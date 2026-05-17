using System.Text.Json;
using Api.Application.Common.Exceptions;
using Infrastructure.Entities;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tasks.DeleteTask;

public class DeleteTaskHandler(
    IKanbanRepository kanbans,
    IKanbanTaskRepository tasks,
    ITeamMemberRepository members,
    IOutboxRepository outboxes,
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

        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = EventType.TaskDeleted,
            Payload = JsonSerializer.Serialize(new
            {
                TaskId = task.Id,
                KanbanId = kanban.Id,
                ColumnId = task.ColumnId,
                UserId = task.UserId,
                DeletedBy = command.CurrentUserId,
                OccuredAt = DateTime.UtcNow,
            }),
            Status = "Pending",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        await outboxes.AddAsync(outboxEvent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
