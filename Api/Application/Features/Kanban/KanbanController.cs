using Api.Application.Common.Extensions;
using Api.Application.Features.Kanban.ChangeKanban;
using Api.Application.Features.Kanban.CreateKanban;
using Api.Application.Features.Kanban.DeleteKanban;
using Api.Application.Features.Kanban.GetKanban;
using Api.Application.Features.Kanban.GetProjectKanbans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Application.Features.Kanban;

[ApiController]
[Route("api")]
[Authorize(Policy = "RequireUserId")]
public class KanbanController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create kanban board
    /// </summary>
    [HttpPost("projects/{projectId:guid}/kanbans")]
    public async Task<IActionResult> CreateKanban(
        Guid projectId,
        [FromBody] CreateKanbanRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var command = new CreateKanbanCommand(projectId, currentUserId, request.Name);
        var result = await mediator.Send(command, cancellationToken);

        return Created($"api/kanbans/{result.Id}", result);
    }

    /// <summary>
    /// Delete kanban board
    /// </summary>
    [HttpDelete("kanbans/{kanbanId:guid}")]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> DeleteKanban(Guid kanbanId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        await mediator.Send(new DeleteKanbanCommand(kanbanId, currentUserId), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Change kanban name
    /// </summary>
    [HttpPut("kanbans/{kanbanId:guid}")]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> ChangeKanban(
        Guid kanbanId,
        [FromBody] ChangeKanbanRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var command = new ChangeKanbanCommand(kanbanId, request.Name, currentUserId);
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get all project kanbans
    /// </summary>
    [HttpGet("projects/{projectId:guid}/kanbans")]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> GetKanbans(Guid projectId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await mediator.Send(new GetProjectKanbansQuery(projectId, userId), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get kanban by id
    /// </summary>
    [HttpGet("kanbans/{kanbanId:guid}")]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> GetKanban(Guid kanbanId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await mediator.Send(new GetKanbanQuery(kanbanId, userId), cancellationToken);

        return Ok(result);
    }
}
