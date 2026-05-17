using Api.Application.Features.Notifications.Common;
using MediatR;

namespace Api.Application.Features.Notifications.GetNotification;

public record GetNotificationQuery(Guid NotificationId, Guid CurrentUserId) : IRequest<NotificationResponse>;
