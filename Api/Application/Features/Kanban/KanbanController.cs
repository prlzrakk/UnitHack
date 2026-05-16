using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.Features.Kanban.CreateKanban;

namespace WebApplication1.Application.Features.Kanban;

[ApiController]
[Route("api")]
public class KanbanController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Example
    /// </summary>
    [HttpPost("projects/{projectId}/kanbans")]
    public async Task<IActionResult> CreateKanban(Guid projectId, [FromBody] CreateKanbanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateKanbanCommand(projectId, request.Name);
        var result = await mediator.Send(command, cancellationToken);
        
        return Created($"api/kanbans/{result.Id}", result);
    }
}