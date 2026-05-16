using Api.Application.Common.Extensions;
using Api.Application.Features.Projects.CreateProject;
using Api.Application.Features.Projects.DeleteProject;
using Api.Application.Features.Projects.GetProject;
using Api.Application.Features.Projects.GetTeamProjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Application.Features.Projects;

[ApiController]
[Route("api")]
[Authorize(Policy = "RequireUserId")]
public class ProjectsController(IMediator mediator) : ControllerBase
{
    [HttpPost("teams/{teamId:guid}/projects")]
    public async Task<IActionResult> CreateProject(
        Guid teamId,
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new CreateProjectCommand(teamId, currentUserId, request.Name), cancellationToken);

        return Created($"api/projects/{result.Id}", result);
    }

    [HttpGet("projects/{projectId:guid}")]
    public async Task<IActionResult> GetProject(Guid projectId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new GetProjectQuery(projectId, currentUserId), cancellationToken);

        return Ok(result);
    }

    [HttpGet("teams/{teamId:guid}/projects")]
    public async Task<IActionResult> GetTeamProjects(Guid teamId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new GetTeamProjectsQuery(teamId, currentUserId), cancellationToken);

        return Ok(result);
    }

    [HttpDelete("projects/{projectId:guid}")]
    public async Task<IActionResult> DeleteProject(Guid projectId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        await mediator.Send(new DeleteProjectCommand(projectId, currentUserId), cancellationToken);

        return NoContent();
    }
}
