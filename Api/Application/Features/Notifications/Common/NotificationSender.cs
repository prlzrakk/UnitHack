using Api.Application.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Api.Application.Features.Notifications.Common;

public class NotificationSender : INotificationSender
{
    private readonly IHubContext<NotificationsHub> hub;

    public NotificationSender(IHubContext<NotificationsHub> hub)
    {
        this.hub = hub;
    }
    public Task SendToUserAsync(Guid userId, object notification, CancellationToken cancellationToken = default)
    {
        return hub.Clients
            .User(userId.ToString())
            .SendAsync("notification.received", notification, cancellationToken);
    }
}
