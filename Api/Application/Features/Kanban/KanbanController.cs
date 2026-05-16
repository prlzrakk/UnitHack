using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.Features.Kanban.ChangeKanban;
using WebApplication1.Application.Features.Kanban.CreateKanban;
using WebApplication1.Application.Features.Kanban.DeleteKanban;

namespace WebApplication1.Application.Features.Kanban;

[ApiController]
[Route("api")]
public class KanbanController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create kanban board
    /// </summary>
    [HttpPost("projects/{projectId:guid}/kanbans")]
    public async Task<IActionResult> CreateKanban(Guid projectId, [FromBody] CreateKanbanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateKanbanCommand(projectId, request.Name);
        var result = await mediator.Send(command, cancellationToken);

        return Created($"api/kanbans/{result.Id}", result);
    }

    /// <summary>
    /// Delete kanban board
    /// </summary>
    [HttpDelete("kanbans/{kanbanId:guid}")]
    public async Task<IActionResult> DeleteKanban(Guid kanbanId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteKanbanCommand(kanbanId), cancellationToken);

        return NoContent();
    }

    [HttpPut("kanbans/{kanbanId:guid}")]
    public async Task<IActionResult> ChangeKanban(Guid kanbanId, [FromBody] ChangeKanbanRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = Guid.Parse(HttpContext.User.Identity.Name);

        var command = new ChangeKanbanCommand(kanbanId, request.Name, currentUserId);

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}