using System.Text.Json;
using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Enums;
using Infrastructure.RabbitMq;
using Infrastructure.RabbitMq.Messages;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Workers;

public class TaskEnrichmentWorker(ILogger<TaskEnrichmentWorker> logger, IServiceScopeFactory serviceScopeFactory, IRabbitMqConsumer consumer) : BackgroundService
{
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.ConsumeAsync(
            queueName: RabbitMqRoutingKeys.TaskEnrichmentQueue,
            routingKey: RabbitMqRoutingKeys.TaskRawCreated,
            handleMessage: async (messageBody, _) =>
            {
                await HandleMessageAsync(messageBody, stoppingToken);
            },
            cancellationToken: stoppingToken
        );
    }

    private async Task HandleMessageAsync(string messageBody, CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var message = JsonSerializer.Deserialize<RawTaskCreatedMessage>(
            messageBody,
            JsonOptions);

        if (message is null)
        {
            throw new InvalidOperationException("Invalid raw task message.");
        }

        ValidateMessage(message);
        
        var alreadyProcessed = await db.ProcessedMessages.AnyAsync(
            x => x.MessageId == message.MessageId &&
                 x.Handler == nameof(TaskEnrichmentWorker),
            stoppingToken);

        if (alreadyProcessed)
        {
            logger.LogInformation(
                "Message {MessageId} was already processed by {Handler}",
                message.MessageId,
                nameof(TaskEnrichmentWorker));

            return;
        }
        
        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == message.ProjectId, stoppingToken);
        if (project is null)
        {
            throw new InvalidOperationException($"Project {message.ProjectId} not found.");
        }
        
        var user = await db.Users
            .FirstOrDefaultAsync(x => x.Id == message.CreatedByUserId, stoppingToken);
        if (user is null)
        {
            throw new InvalidOperationException($"User {message.CreatedByUserId} not found.");
        }
        
        var kanban = await db.Kanbans
            .FirstOrDefaultAsync(x => x.ProjectId == project.Id, stoppingToken);
        if (kanban is null)
        {
            throw new InvalidOperationException($"Kanban for project {project.Id} not found.");
        }
        
        var defaultColumn = await db.KanbanColumns
            .Where(x => x.KanbanId == kanban.Id)
            .OrderBy(x => x.Order)
            .FirstOrDefaultAsync(stoppingToken);
        if (defaultColumn is null)
        {
            throw new InvalidOperationException($"Default column for kanban {kanban.Id} not found.");
        }
        
        var taskId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var priority = ParsePriority(message.Priority);

        var task = new KanbanTask
        {
            Id = taskId,
            Name = message.Title,
            Description = message.Description,
            ColumnId = defaultColumn.Id,
            Priority = priority,
            CreatedAt = createdAt
        };

        db.Tasks.Add(task);
        db.ProcessedMessages.Add(new ProcessedMessage
        {
            Id = Guid.NewGuid(),
            MessageId = message.MessageId,
            Handler = nameof(TaskEnrichmentWorker),
            ProcessedAt = createdAt
        });
        
        var enrichedEvent = new TaskEnrichedCreatedEvent(
            MessageId: message.MessageId,
            TaskId: taskId,
            ProjectId: project.Id,
            ProjectName: project.Name,
            CreatedByUserId: user.Id,
            CreatedByUserName: user.Name,
            ColumnId: defaultColumn.Id,
            ColumnName: defaultColumn.Name,
            Title: message.Title,
            Description: message.Description,
            Priority: priority.ToString(),
            Tags: message.Tags,
            CreatedAt: createdAt,
            Deadline: message.Deadline
        );
        
        db.OutboxEvents.Add(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = EventType.IncomingTaskProceeded,
            Payload = JsonSerializer.Serialize(enrichedEvent, JsonOptions),
            CreatedAt = createdAt,
            Status = "Pending",
            RetryCount = 0
        });
        
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.SaveChangesAsync(stoppingToken);
        logger.LogInformation(
            "Raw task message {MessageId} enriched into task {TaskId}",
            message.MessageId,
            taskId);
    }

    private void ValidateMessage(RawTaskCreatedMessage message)
    {
        if (message.MessageId == Guid.Empty)
        {
            throw new InvalidOperationException("MessageId is required.");
        }

        if (message.ProjectId == Guid.Empty)
        {
            throw new InvalidOperationException("ProjectId is required.");
        }

        if (message.CreatedByUserId == Guid.Empty)
        {
            throw new InvalidOperationException("CreatedByUserId is required.");
        }

        if (string.IsNullOrWhiteSpace(message.Title))
        {
            throw new InvalidOperationException("Task title is required.");
        }
    }

    private static Priority ParsePriority(string? priority)
    {
        if (Enum.TryParse<Priority>(priority, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        return Priority.Medium;
    }
}