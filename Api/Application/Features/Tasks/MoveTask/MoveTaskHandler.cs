using System.Text.Json;
using Api.Application.Common.Exceptions;
using Api.Application.Features.Tags.Common;
using Api.Application.Features.Tasks.Common;
using Infrastructure.Entities;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tasks.MoveTask;

public class MoveTaskHandler(
    IKanbanRepository kanbans,
    IKanbanColumnRepository columns,
    IKanbanTaskRepository tasks,
    ITaskTagRepository taskTags,
    ITeamMemberRepository members,
    IOutboxRepository outboxes,
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
        var fromColumnId = task.ColumnId;
        task.Column?.Tasks.Remove(task);
        task.ColumnId = toColumn.Id;
        task.Column = toColumn;
        task.Order = command.Order;
        toColumn.Tasks ??= [];
        if (!toColumn.Tasks.Contains(task))
            toColumn.Tasks.Add(task);

        var responseTags = await taskTags.GetTagsByTaskIdAsync(task.Id, cancellationToken);

        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = EventType.TaskMoved,
            Payload = JsonSerializer.Serialize(new
            {
                TaskId = task.Id,
                KanbanId = kanban.Id,
                ToColumnId = toColumn.Id,
                FromColumnId = fromColumnId,
                Order = command.Order,
                MovedBy = command.CurrentUserId,
                OccuredAt = DateTime.UtcNow,
            }),
            Status = "Pending",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        await outboxes.AddAsync(outboxEvent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(task, responseTags);
    }

    private static TaskResponse ToResponse(KanbanTask task, IEnumerable<Tag> tags) => new(
        task.Id,
        task.KanbanId,
        task.ColumnId,
        task.UserId,
        task.Name,
        task.Description,
        task.Priority,
        task.CreatedAt,
        task.Deadline,
        task.Order,
        tags.ToResponseList());
}
