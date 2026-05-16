using Api.Application.Common.Extensions;
using Api.Application.Features.Teams.AddTeamMember;
using Api.Application.Features.Teams.ChangeMemberRole;
using Api.Application.Features.Teams.CreateTeam;
using Api.Application.Features.Teams.GetTeam;
using Api.Application.Features.Teams.GetTeams;
using Api.Application.Features.Teams.RemoveTeamMember;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Application.Features.Teams;


[ApiController]
[Route("api/teams")]
public class TeamsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> GetTeams(CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new GetTeamsQuery(currentUserId), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{teamId:guid}")]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> GetTeam(Guid teamId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new GetTeamQuery(teamId, currentUserId), cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest req, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await mediator.Send(new CreateTeamCommand(userId, req.Name), cancellationToken);
        
        return Created($"api/teams/{result.Id}", result);
    }

    [HttpPost("{teamId:guid}/members")]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> AddMember(
        Guid teamId,
        [FromBody] AddTeamMemberRequest req,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        var result = await mediator.Send(new AddTeamMemberCommand(teamId, currentUserId, req.UserId), cancellationToken);

        return Created($"api/teams/{teamId}/members/{result.UserId}", result);
    }

    [HttpDelete("{teamId:guid}/members/{userId:guid}")]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> RemoveMember(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        await mediator.Send(new RemoveTeamMemberCommand(teamId, currentUserId, userId), cancellationToken);

        return NoContent();
    }
    
    [HttpPatch("{teamId:guid}/members/{userId:guid}/role")]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> ChangeMemberRole(
        Guid teamId,
        Guid userId,
        [FromBody] ChangeMemberRoleRequest req,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (req.Role is null)
            return BadRequest("Role is required");

        var result = await mediator.Send(
            new ChangeMemberRoleCommand(teamId, currentUserId, userId, req.Role.Value),
            cancellationToken);

        return Ok(result);
    }
}

