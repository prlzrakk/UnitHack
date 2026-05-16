using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockNotificationRepository(MockDataStore store) : INotificationRepository
{
    public Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = store.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return Task.FromResult(notifications);
    }

    public Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = store.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return Task.FromResult(notifications);
    }

    public Task<Notification?> GetByNotificationIdAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = store.Notifications.FirstOrDefault(x => x.Id == notificationId);
        return Task.FromResult(notification);
    }

    public Task<Notification> AddAsync(
        Guid userId,
        Guid taskId,
        Guid kanbanId,
        string name,
        string? message,
        CancellationToken cancellationToken)
    {
        var user = new User { Id = userId };
        var task = store.Tasks.FirstOrDefault(x => x.Id == taskId);
        var kanban = store.Kanbans.FirstOrDefault(x => x.Id == kanbanId);
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            TaskId = taskId,
            KanbanTask = task,
            KanbanId = kanbanId,
            Kanban = kanban,
            Name = name.Trim(),
            Message = string.IsNullOrWhiteSpace(message) ? string.Empty : message.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        store.Notifications.Add(notification);
        return Task.FromResult(notification);
    }

    public Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
