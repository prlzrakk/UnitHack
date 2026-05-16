using Api.Application.Common.Exceptions;
using Api.Application.Features.Notifications.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Notifications.GetNotification;

public class GetNotificationHandler(INotificationRepository notifications)
    : IRequestHandler<GetNotificationQuery, NotificationResponse>
{
    public async Task<NotificationResponse> Handle(
        GetNotificationQuery query,
        CancellationToken cancellationToken)
    {
        var notification = await notifications.GetByNotificationIdAsync(query.NotificationId, cancellationToken)
                           ?? throw new NotFoundException("Notification not found");

        if (notification.UserId != query.CurrentUserId)
            throw new ForbiddenException("Only notification owner can view notification");

        return notification.ToResponse();
    }
}
