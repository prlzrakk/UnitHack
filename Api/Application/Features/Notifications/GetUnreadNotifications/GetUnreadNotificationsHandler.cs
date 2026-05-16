using Api.Application.Features.Notifications.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Notifications.GetUnreadNotifications;

public class GetUnreadNotificationsHandler(INotificationRepository notifications)
    : IRequestHandler<GetUnreadNotificationsQuery, List<NotificationResponse>>
{
    public async Task<List<NotificationResponse>> Handle(
        GetUnreadNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        var unreadNotifications = await notifications.GetUnreadByUserIdAsync(query.CurrentUserId, cancellationToken);
        return unreadNotifications.ToResponseList();
    }
}
