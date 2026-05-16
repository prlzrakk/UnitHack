using Api.Application.Common.Exceptions;
using Api.Application.Features.Projects.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Projects.GetTeamProjects;

public class GetTeamProjectsHandler(
    ITeamRepository teams,
    ITeamMemberRepository members,
    IProjectRepository projects) : IRequestHandler<GetTeamProjectsQuery, List<ProjectResponse>>
{
    public async Task<List<ProjectResponse>> Handle(GetTeamProjectsQuery query, CancellationToken cancellationToken)
    {
        if (query.TeamId == Guid.Empty)
            throw new BadRequestException("Team id is required");
        if (query.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");

        var team = await teams.GetTeam(query.TeamId, cancellationToken)
                   ?? throw new NotFoundException("Team not found");

        var isMember = await members.IsMemberAsync(team.Id, query.CurrentUserId, cancellationToken);
        if (!isMember)
            throw new ForbiddenException("Only team member can view projects");

        var result = await projects.GetByTeamIdAsync(team.Id, cancellationToken);
        return result
            .Select(x => new ProjectResponse(x.Id, x.TeamId, x.Name))
            .ToList();
    }
}
