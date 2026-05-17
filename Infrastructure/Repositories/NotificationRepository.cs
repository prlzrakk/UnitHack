using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class NotificationRepository(DatabaseContext context) : INotificationRepository
{
    public async Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification?> GetByNotificationIdAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        return await context.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
    }

    public async Task<Notification> AddAsync(
        Guid userId,
        Guid taskId,
        Guid kanbanId,
        string name,
        string? message,
        CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaskId = taskId,
            KanbanId = kanbanId,
            Name = name.Trim(),
            Message = string.IsNullOrWhiteSpace(message) ? string.Empty : message.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await context.Notifications.AddAsync(notification, cancellationToken);
        return notification;
    }

    public Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
    {
        context.Notifications.Update(notification);
        return Task.CompletedTask;
    }
}
