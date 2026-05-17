using Api.Application.Common.Extensions;
using Api.Application.Features.Tags.CreateTag;
using Api.Application.Features.Tags.DeleteTag;
using Api.Application.Features.Tags.GetKanbanTags;
using Api.Application.Features.Tags.RenameTag;
using Api.Application.Features.TaskTags.AttachTaskTag;
using Api.Application.Features.TaskTags.DetachTaskTag;
using Api.Application.Features.TaskTags.GetTaskTags;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Application.Features.Tags;

[ApiController]
[Route("api")]
[Authorize(Policy = "RequireUserId")]
public class TagsController(IMediator mediator) : ControllerBase
{
    [HttpGet("kanbans/{kanbanId:guid}/tags")]
    public async Task<IActionResult> GetKanbanTags(Guid kanbanId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new GetKanbanTagsQuery(kanbanId, currentUserId), cancellationToken);

        return Ok(result);
    }

    [HttpPost("kanbans/{kanbanId:guid}/tags")]
    public async Task<IActionResult> CreateTag(
        Guid kanbanId,
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new CreateTagCommand(kanbanId, currentUserId, request.Name), cancellationToken);

        return Created($"api/tags/{result.Id}", result);
    }

    [HttpPut("tags/{tagId:guid}")]
    public async Task<IActionResult> RenameTag(
        Guid tagId,
        [FromBody] RenameTagRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new RenameTagCommand(tagId, currentUserId, request.Name), cancellationToken);

        return Ok(result);
    }

    [HttpDelete("tags/{tagId:guid}")]
    public async Task<IActionResult> DeleteTag(Guid tagId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        await mediator.Send(new DeleteTagCommand(tagId, currentUserId), cancellationToken);

        return NoContent();
    }

    [HttpGet("tasks/{taskId:guid}/tags")]
    public async Task<IActionResult> GetTaskTags(Guid taskId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new GetTaskTagsQuery(taskId, currentUserId), cancellationToken);

        return Ok(result);
    }

    [HttpPost("tasks/{taskId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> AttachTaskTag(
        Guid taskId,
        Guid tagId,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new AttachTaskTagCommand(taskId, tagId, currentUserId), cancellationToken);

        return Ok(result);
    }

    [HttpDelete("tasks/{taskId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> DetachTaskTag(
        Guid taskId,
        Guid tagId,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        await mediator.Send(new DetachTaskTagCommand(taskId, tagId, currentUserId), cancellationToken);

        return NoContent();
    }
}
