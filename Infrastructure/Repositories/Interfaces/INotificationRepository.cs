using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Notification?> GetByNotificationIdAsync(Guid notificationId, CancellationToken cancellationToken);

    Task<Notification> AddAsync(
        Guid userId,
        Guid taskId,
        Guid kanbanId,
        string name,
        string? message,
        CancellationToken cancellationToken);

    Task UpdateAsync(Notification notification, CancellationToken cancellationToken);
}
