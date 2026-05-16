using Api.Application.Common.Exceptions;
using Api.Application.Features.Projects.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Projects.CreateProject;

public class CreateProjectHandler(
    ITeamRepository teams,
    ITeamMemberRepository members,
    IProjectRepository projects,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProjectCommand, ProjectResponse>
{
    public async Task<ProjectResponse> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var team = await teams.GetTeam(command.TeamId, cancellationToken)
                   ?? throw new NotFoundException("Team not found");

        var isAdmin = await members.IsAdminAsync(team.Id, command.CurrentUserId, cancellationToken);
        if (!isAdmin)
            throw new ForbiddenException("Only team admin can create projects");

        var project = await projects.AddAsync(team.Id, command.Name, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProjectResponse(project.Id, project.TeamId, project.Name);
    }
}
