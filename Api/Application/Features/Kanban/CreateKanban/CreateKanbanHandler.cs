using Api.Application.Common.Exceptions;
using Api.Application.Features.Notifications.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.CreateKanban;

public class CreateKanbanHandler(
    IProjectRepository projectRepository,
    ITeamMemberRepository teamMemberRepository,
    IKanbanRepository kanbanRepository,
    IUnitOfWork unitOfWork,
    INotificationSender notificationSender
) : IRequestHandler<CreateKanbanCommand, CreateKanbanResponse>
{
    public async Task<CreateKanbanResponse> Handle(CreateKanbanCommand request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetProjectById(request.ProjectId, cancellationToken);

        if (project is null)
            throw new NotFoundException("Project not found");

        var isAdmin = await teamMemberRepository.IsAdminAsync(project.TeamId, request.CurrentUserId, cancellationToken);
        if (!isAdmin)
            throw new ForbiddenException("Only team admin can create kanban");

        var kanban = await kanbanRepository.AddAsync(project.Id, request.Name, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await NotifyTeamMembersAsync(project.TeamId, project.Id, kanban.Id, kanban.Name, cancellationToken);

        return new CreateKanbanResponse
        {
            Id = kanban.Id,
            ProjectId = kanban.ProjectId,
            Name = kanban.Name,
            Columns = kanban.Columns
                .OrderBy(x => x.Order)
                .Select(x => new KanbanColumnResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Order = x.Order
                })
                .ToList()
        };
    }

    private async Task NotifyTeamMembersAsync(
        Guid teamId,
        Guid projectId,
        Guid kanbanId,
        string kanbanName,
        CancellationToken cancellationToken)
    {
        var teamMembers = await teamMemberRepository.GetMembersByTeamIdAsync(teamId, cancellationToken);

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
                    kanbanId,
                    type = "KanbanCreated",
                    name = "Kanban Created",
                    message = $"Создан канбан «{kanbanName}»",
                    isRead = false,
                    isPersisted = false,
                    createdAt = DateTime.UtcNow
                },
                cancellationToken);
        }
    }
}
