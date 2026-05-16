using Api.Application.Common.Exceptions;
using Api.Application.Features.Tasks.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tasks.UpdateTask;

public class UpdateTaskHandler(
    IKanbanRepository kanbans,
    IKanbanTaskRepository tasks,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateTaskCommand, TaskResponse>
{
    public async Task<TaskResponse> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        if (command.TaskId == Guid.Empty)
            throw new BadRequestException("Task id is required");
        if (command.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new BadRequestException("Task name is required");
        if (command.UserId == Guid.Empty)
            throw new BadRequestException("User id is required");
        if (!Enum.IsDefined(command.Priority))
            throw new BadRequestException("Priority is invalid");

        var task = await tasks.GetByIdAsync(command.TaskId, cancellationToken)
                   ?? throw new NotFoundException("Task not found");
        var kanban = await kanbans.GetByIdWithProjectAsync(task.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        var teamId = kanban.Project.TeamId;
        if (!await members.IsMemberAsync(teamId, command.CurrentUserId, cancellationToken))
            throw new ForbiddenException("Only team member can update tasks");
        if (!await members.IsMemberAsync(teamId, command.UserId, cancellationToken))
            throw new BadRequestException("Assignee is not a team member");

        task.Name = command.Name.Trim();
        task.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        task.Priority = command.Priority;
        task.Deadline = command.Deadline;
        task.UserId = command.UserId;

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
