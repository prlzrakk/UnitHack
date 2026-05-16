using Api.Application.Common.Exceptions;
using Api.Application.Features.Projects.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Projects.GetProject;

public class GetProjectHandler(IProjectRepository projects, ITeamMemberRepository members)
    : IRequestHandler<GetProjectQuery, ProjectResponse>
{
    public async Task<ProjectResponse> Handle(GetProjectQuery query, CancellationToken cancellationToken)
    {
        if (query.ProjectId == Guid.Empty)
            throw new BadRequestException("Project id is required");
        if (query.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");

        var project = await projects.GetProjectById(query.ProjectId, cancellationToken)
                      ?? throw new NotFoundException("Project not found");

        var isMember = await members.IsMemberAsync(project.TeamId, query.CurrentUserId, cancellationToken);
        if (!isMember)
            throw new ForbiddenException("Only team member can view project");

        return new ProjectResponse(project.Id, project.TeamId, project.Name);
    }
}
