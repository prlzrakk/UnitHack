using Api.Application.Features.Notifications.Common;
using MediatR;

namespace Api.Application.Features.Notifications.GetNotifications;

public record GetNotificationsQuery(Guid CurrentUserId) : IRequest<List<NotificationResponse>>;
