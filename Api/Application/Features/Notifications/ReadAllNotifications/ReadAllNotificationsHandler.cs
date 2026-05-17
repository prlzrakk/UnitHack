using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Notifications.ReadAllNotifications;

public class ReadAllNotificationsHandler(
    INotificationRepository notifications,
    IUnitOfWork unitOfWork) : IRequestHandler<ReadAllNotificationsCommand, ReadAllNotificationsResponse>
{
    public async Task<ReadAllNotificationsResponse> Handle(
        ReadAllNotificationsCommand command,
        CancellationToken cancellationToken)
    {
        var unreadNotifications = await notifications.GetUnreadByUserIdAsync(command.CurrentUserId, cancellationToken);
        var readAt = DateTime.UtcNow;

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = readAt;
            await notifications.UpdateAsync(notification, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReadAllNotificationsResponse(unreadNotifications.Count);
    }
}
