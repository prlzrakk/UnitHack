using MediatR;

namespace Api.Application.Features.Notifications.ReadAllNotifications;

public record ReadAllNotificationsCommand(Guid CurrentUserId) : IRequest<ReadAllNotificationsResponse>;
