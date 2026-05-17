using System.Text.Json;
using Infrastructure.Enums;
using Infrastructure.RabbitMq;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Workers;

public class NotificationWorker(IRabbitMqConsumer consumer, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return consumer.ConsumeAsync(
            queueName: "notifications:events",
            routingKey: "#",
            handleMessage: (eventType, payload) => HandleMessageAsync(eventType, payload, stoppingToken),
            cancellationToken: stoppingToken
        );
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

    }

    private (string name, string message) BuildNotificationMessage(string eventType)
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