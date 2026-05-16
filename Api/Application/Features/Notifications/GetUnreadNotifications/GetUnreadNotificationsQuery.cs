using Api.Application.Features.Notifications.Common;
using MediatR;

namespace Api.Application.Features.Notifications.GetUnreadNotifications;

public record GetUnreadNotificationsQuery(Guid CurrentUserId) : IRequest<List<NotificationResponse>>;
