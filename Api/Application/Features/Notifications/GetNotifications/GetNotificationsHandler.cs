using Api.Application.Features.Notifications.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Notifications.GetNotifications;

public class GetNotificationsHandler(INotificationRepository notifications)
    : IRequestHandler<GetNotificationsQuery, List<NotificationResponse>>
{
    public async Task<List<NotificationResponse>> Handle(
        GetNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        var userNotifications = await notifications.GetByUserIdAsync(query.CurrentUserId, cancellationToken);
        return userNotifications.ToResponseList();
    }
}
