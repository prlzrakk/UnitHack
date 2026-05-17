using System.Text.Json;
using Api.Application.Features.Notifications.Common;
using Infrastructure.Enums;
using Infrastructure.RabbitMq;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Workers;

public class NotificationWorker(
    IRabbitMqConsumer consumer,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationWorker> logger) : BackgroundService
{
    private const int RetryDelayMilliseconds = 5000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await consumer.ConsumeAsync(
                    queueName: "notifications:events",
                    routingKey: "#",
                    handleMessage: (eventType, payload) => HandleMessageAsync(eventType, payload, stoppingToken),
                    cancellationToken: stoppingToken
                );
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Notification worker failed. It will retry RabbitMQ subscription in 5 seconds.");

                try
                {
                    await Task.Delay(RetryDelayMilliseconds, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }

    private async Task HandleMessageAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        if (eventType == EventType.TaskDeleted.ToString())
            return;

        if (eventType != EventType.TaskCreated.ToString()
            && eventType != EventType.TaskUpdated.ToString()
            && eventType != EventType.TaskMoved.ToString())
            return;

        using var scope = scopeFactory.CreateScope();
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var userId = root.GetProperty("UserId").GetGuid();
        var taskId = root.GetProperty("TaskId").GetGuid();
        var kanbanId = root.GetProperty("KanbanId").GetGuid();

        var (name, message) = BuildNotificationMessage(eventType);
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await notifications.AddAsync(userId, taskId, kanbanId, name, message, cancellationToken);
        var notificationSender = scope.ServiceProvider.GetRequiredService<INotificationSender>();
        var notification = await notifications.AddAsync(
            userId,
            taskId,
            kanbanId,
            name,
            message,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notificationSender.SendToUserAsync(
            userId,
            new
            {
                id = notification.Id,
                userId = userId,
                taskId = taskId,
                kanbanId = kanbanId,
                name = name,
                message = message,
                isRead = false,
                createdAt = DateTime.UtcNow
            },
            cancellationToken);
    }

    private static (string name, string message) BuildNotificationMessage(string eventType)
    {
        if (eventType == EventType.TaskCreated.ToString())
            return ("Task Created", "Вам было назначено новое задание");

        if (eventType == EventType.TaskUpdated.ToString())
            return ("Task Updated", "Одно из ваших заданий было обновлено");

        if (eventType == EventType.TaskMoved.ToString())
            return ("Task Moved", "Одно из ваших заданий было перенесено в другую колонку");

        return ("Task event", "Что-то в задании было изменено");
    }
}
