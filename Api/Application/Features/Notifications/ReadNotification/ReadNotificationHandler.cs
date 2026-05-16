using Api.Application.Common.Exceptions;
using Api.Application.Features.Notifications.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Notifications.ReadNotification;

public class ReadNotificationHandler(
    INotificationRepository notifications,
    IUnitOfWork unitOfWork) : IRequestHandler<ReadNotificationCommand, NotificationResponse>
{
    public async Task<NotificationResponse> Handle(
        ReadNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var notification = await notifications.GetByNotificationIdAsync(command.NotificationId, cancellationToken)
                           ?? throw new NotFoundException("Notification not found");

        if (notification.UserId != command.CurrentUserId)
            throw new ForbiddenException("Only notification owner can read notification");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await notifications.UpdateAsync(notification, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return notification.ToResponse();
    }
}
