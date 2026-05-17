using Api.Application.Common.Exceptions;
using Api.Application.Features.Notifications.Common;
using Api.Application.Features.Projects.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Projects.CreateProject;

public class CreateProjectHandler(
    ITeamRepository teams,
    ITeamMemberRepository members,
    IProjectRepository projects,
    IUnitOfWork unitOfWork,
    INotificationSender notificationSender) : IRequestHandler<CreateProjectCommand, ProjectResponse>
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
        await NotifyTeamMembersAsync(team.Id, project.Id, project.Name, cancellationToken);

        return new ProjectResponse(project.Id, project.TeamId, project.Name);
    }

    private async Task NotifyTeamMembersAsync(
        Guid teamId,
        Guid projectId,
        string projectName,
        CancellationToken cancellationToken)
    {
        var teamMembers = await members.GetMembersByTeamIdAsync(teamId, cancellationToken);

        foreach (var member in teamMembers)
        {
            await notificationSender.SendToUserAsync(
                member.UserId,
                new
                {
                    id = Guid.NewGuid(),
                    userId = member.UserId,
                    teamId,
                    projectId,
                    type = "ProjectCreated",
                    name = "Project Created",
                    message = $"Создан проект «{projectName}»",
                    isRead = false,
                    isPersisted = false,
                    createdAt = DateTime.UtcNow
                },
                cancellationToken);
        }
    }
}
