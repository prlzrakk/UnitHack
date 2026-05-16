using Api.Application.Common.Exceptions;
using Api.Application.Features.Tasks.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tasks.MoveTask;

public class MoveTaskHandler(
    IKanbanRepository kanbans,
    IKanbanColumnRepository columns,
    IKanbanTaskRepository tasks,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<MoveTaskCommand, TaskResponse>
{
    public async Task<TaskResponse> Handle(MoveTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(command.TaskId, cancellationToken)
                   ?? throw new NotFoundException("Task not found");
        var kanban = await kanbans.GetByIdWithProjectAsync(task.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");
        var toColumn = await columns.GetByIdAsync(command.ToColumnId, cancellationToken)
                       ?? throw new NotFoundException("Column not found");

        if (toColumn.KanbanId != task.KanbanId)
            throw new BadRequestException("Column does not belong to task kanban");

        var teamId = kanban.Project.TeamId;
        if (!await members.IsMemberAsync(teamId, command.CurrentUserId, cancellationToken))
            throw new ForbiddenException("Only team member can move tasks");

        task.Column?.Tasks.Remove(task);
        task.ColumnId = toColumn.Id;
        task.Column = toColumn;
        task.Order = command.Order;
        toColumn.Tasks ??= [];
        if (!toColumn.Tasks.Contains(task))
            toColumn.Tasks.Add(task);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(task);
    }

    private static TaskResponse ToResponse(KanbanTask task) => new(
        task.Id,
        task.KanbanId,
        task.ColumnId,
        task.UserId,
        task.Name,
        task.Description,
        task.Priority,
        task.CreatedAt,
        task.Deadline,
        task.Order);
}
