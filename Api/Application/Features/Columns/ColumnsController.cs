using Api.Application.Common.Extensions;
using Api.Application.Features.Columns.CreateColumn;
using Api.Application.Features.Columns.DeleteColumn;
using Api.Application.Features.Columns.RenameColumn;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Application.Features.Columns;

[ApiController]
[Route("api")]
[Authorize(Policy = "RequireUserId")]
public class ColumnsController(IMediator mediator) : ControllerBase
{
    [HttpPost("kanbans/{kanbanId:guid}/columns")]
    public async Task<IActionResult> CreateColumn(
        Guid kanbanId,
        [FromBody] CreateColumnRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(
            new CreateColumnCommand(kanbanId, currentUserId, request.Name, request.Order),
            cancellationToken);

        return Created($"api/columns/{result.Id}", result);
    }

    [HttpPut("columns/{columnId:guid}")]
    public async Task<IActionResult> RenameColumn(
        Guid columnId,
        [FromBody] RenameColumnRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new RenameColumnCommand(columnId, currentUserId, request.Name), cancellationToken);

        return Ok(result);
    }

    [HttpDelete("columns/{columnId:guid}")]
    public async Task<IActionResult> DeleteColumn(Guid columnId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        await mediator.Send(new DeleteColumnCommand(columnId, currentUserId), cancellationToken);

        return NoContent();
    }
}
