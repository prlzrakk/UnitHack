using Api.Application.Common.Exceptions;
using Api.Application.Features.Teams.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Teams.GetTeam;

public class GetTeamHandler(ITeamRepository teams, ITeamMemberRepository members)
    : IRequestHandler<GetTeamQuery, TeamDetailsResponse>
{
    public async Task<TeamDetailsResponse> Handle(GetTeamQuery query, CancellationToken cancellationToken)
    {
        var team = await teams.GetTeam(query.TeamId, cancellationToken)
                   ?? throw new NotFoundException("Team not found");

        var currentUserIsMember = await members.IsMemberAsync(team.Id, query.CurrentUserId, cancellationToken);
        if (!currentUserIsMember)
            throw new ForbiddenException("Only team member can view team");

        var teamMembers = await members.GetMembersByTeamIdAsync(team.Id, cancellationToken);

        return new TeamDetailsResponse(
            team.Id,
            team.Name,
            teamMembers
                .Select(x => new TeamMemberResponse(x.UserId, x.Role))
                .ToList());
    }
}
