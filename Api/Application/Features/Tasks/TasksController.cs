using Api.Application.Common.Extensions;
using Api.Application.Features.Tasks.CreateTask;
using Api.Application.Features.Tasks.DeleteTask;
using Api.Application.Features.Tasks.MoveTask;
using Api.Application.Features.Tasks.UpdateTask;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Application.Features.Tasks;

[ApiController]
[Route("api")]
[Authorize(Policy = "RequireUserId")]
public class TasksController(IMediator mediator) : ControllerBase
{
    [HttpPost("kanbans/{kanbanId:guid}/tasks")]
    public async Task<IActionResult> CreateTask(
        Guid kanbanId,
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new CreateTaskCommand(
            kanbanId,
            currentUserId,
            request.Name,
            request.Description,
            request.Priority,
            request.Deadline,
            request.UserId,
            request.ColumnId,
            request.Order), cancellationToken);

        return Created($"api/tasks/{result.Id}", result);
    }

    [HttpPut("tasks/{taskId:guid}")]
    public async Task<IActionResult> UpdateTask(
        Guid taskId,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new UpdateTaskCommand(
            taskId,
            currentUserId,
            request.Name,
            request.Description,
            request.Priority,
            request.Deadline,
            request.UserId), cancellationToken);

        return Ok(result);
    }

    [HttpPatch("tasks/{taskId:guid}")]
    public async Task<IActionResult> MoveTask(
        Guid taskId,
        [FromBody] MoveTaskRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(
            new MoveTaskCommand(taskId, currentUserId, request.ToColumnId, request.Order),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("tasks/{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid taskId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        await mediator.Send(new DeleteTaskCommand(taskId, currentUserId), cancellationToken);

        return NoContent();
    }
}
