namespace Api.Application.Features.Notifications.Common;

public interface INotificationSender
{
    Task SendToUserAsync(Guid userId, object notification, CancellationToken cancellationToken = default);
}