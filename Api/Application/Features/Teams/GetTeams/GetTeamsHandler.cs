using Api.Application.Features.Teams.Common;
using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Teams.GetTeams;

public class GetTeamsHandler(ITeamRepository teams, ITeamMemberRepository members)
    : IRequestHandler<GetTeamsQuery, List<TeamListItemResponse>>
{
    public async Task<List<TeamListItemResponse>> Handle(GetTeamsQuery query, CancellationToken cancellationToken)
    {
        if (query.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");

        var userTeams = await teams.GetTeamsByUserId(query.CurrentUserId, cancellationToken);
        var result = new List<TeamListItemResponse>();

        foreach (var team in userTeams)
        {
            var member = await members.GetMemberAsync(team.Id, query.CurrentUserId, cancellationToken);
            if (member is not null)
                result.Add(new TeamListItemResponse(team.Id, team.Name, member.Role));
        }

        return result;
    }
}
