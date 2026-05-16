using Api.Application.Features.Notifications.Common;
using MediatR;

namespace Api.Application.Features.Notifications.CreateNotification;

public record CreateNotificationCommand(
    Guid UserId,
    Guid TaskId,
    Guid KanbanId,
    string Name,
    string? Message) : IRequest<NotificationResponse>;
