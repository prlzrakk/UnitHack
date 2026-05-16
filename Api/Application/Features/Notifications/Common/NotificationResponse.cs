using Infrastructure.Entities;

namespace Api.Application.Features.Notifications.Common;

public record NotificationResponse(
    Guid Id,
    Guid UserId,
    Guid TaskId,
    Guid KanbanId,
    string Name,
    string Message,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);

public static class NotificationResponseMapper
{
    public static NotificationResponse ToResponse(this Notification notification) => new(
        notification.Id,
        notification.UserId,
        notification.TaskId,
        notification.KanbanId,
        notification.Name,
        notification.Message,
        notification.IsRead,
        notification.CreatedAt,
        notification.ReadAt);

    public static List<NotificationResponse> ToResponseList(this IEnumerable<Notification> notifications) =>
        notifications.Select(notification => notification.ToResponse()).ToList();
}
