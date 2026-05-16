using Api.Application.Features.Notifications.Common;
using MediatR;

namespace Api.Application.Features.Notifications.ReadNotification;

public record ReadNotificationCommand(Guid NotificationId, Guid CurrentUserId) : IRequest<NotificationResponse>;
