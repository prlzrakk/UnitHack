using Api.Application.Common.Exceptions;
using Api.Application.Features.Projects.Common;
using Infrastructure.Entities;
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
        if (command.TeamId == Guid.Empty)
            throw new BadRequestException("Team id is required");
        if (command.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new BadRequestException("Project name is required");

        var team = await teams.GetTeam(command.TeamId, cancellationToken)
                   ?? throw new NotFoundException("Team not found");

        var isAdmin = await members.IsAdminAsync(team.Id, command.CurrentUserId, cancellationToken);
        if (!isAdmin)
            throw new ForbiddenException("Only team admin can create projects");

        var project = await projects.AddAsync(new Project
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            Team = team,
            Name = command.Name.Trim()
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProjectResponse(project.Id, project.TeamId, project.Name);
    }
}
