using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tasks.DeleteTask;

public class DeleteTaskHandler(
    IKanbanRepository kanbans,
    IKanbanTaskRepository tasks,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteTaskCommand>
{
    public async Task Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(command.TaskId, cancellationToken)
                   ?? throw new NotFoundException("Task not found");
        var kanban = await kanbans.GetByIdWithProjectAsync(task.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        var teamId = kanban.Project.TeamId;
        if (!await members.IsMemberAsync(teamId, command.CurrentUserId, cancellationToken))
            throw new ForbiddenException("Only team member can delete tasks");

        await tasks.DeleteAsync(task.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
