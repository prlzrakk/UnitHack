using Api.Application.Common.Extensions;
using Api.Application.Features.Notifications.GetNotification;
using Api.Application.Features.Notifications.GetNotifications;
using Api.Application.Features.Notifications.GetUnreadNotifications;
using Api.Application.Features.Notifications.ReadAllNotifications;
using Api.Application.Features.Notifications.ReadNotification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Application.Features.Notifications;

[ApiController]
[Route("api/notifications")]
[Authorize(Policy = "RequireUserId")]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new GetNotificationsQuery(currentUserId), cancellationToken);

        return Ok(result);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications(CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new GetUnreadNotificationsQuery(currentUserId), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{notificationId:guid}")]
    public async Task<IActionResult> GetNotification(Guid notificationId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(
            new GetNotificationQuery(notificationId, currentUserId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> ReadNotification(Guid notificationId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(
            new ReadNotificationCommand(notificationId, currentUserId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> ReadAllNotifications(CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new ReadAllNotificationsCommand(currentUserId), cancellationToken);

        return Ok(result);
    }
}
