using System.Text.Json;
using Api.Application.Common.Events;
using Api.Application.Common.Exceptions;
using Api.Application.Features.Tags.Common;
using Api.Application.Features.Tasks.Common;
using Infrastructure.Entities;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tasks.UpdateTask;

public class UpdateTaskHandler(
    IKanbanRepository kanbans,
    IKanbanTaskRepository tasks,
    ITagRepository tags,
    ITaskTagRepository taskTags,
    ITeamMemberRepository members,
    IOutboxRepository outboxes,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateTaskCommand, TaskResponse>
{
    public async Task<TaskResponse> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
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

        var responseTags = await GetResponseTagsAsync(task.Id, kanban.Id, command.TagIds, cancellationToken);

        await outboxes.AddAsync(OutboxEventFactory.Create(EventType.TaskUpdated, new
        {
            TaskId = task.Id,
            KanbanId = kanban.Id,
            ColumnId = task.ColumnId,
            CreatedBy = command.CurrentUserId,
            UserId = task.UserId,
            Priority = task.Priority,
            Deadline = task.Deadline,
            UpdatedBy = command.CurrentUserId,
            OccurredAt = DateTime.UtcNow,
        }), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(task, responseTags);
    }

    private async Task<List<Tag>> GetResponseTagsAsync(
        Guid taskId,
        Guid kanbanId,
        IReadOnlyCollection<Guid>? tagIds,
        CancellationToken cancellationToken)
    {
        if (tagIds is null)
            return await taskTags.GetTagsByTaskIdAsync(taskId, cancellationToken);

        var distinctTagIds = tagIds.Distinct().ToArray();
        var selectedTags = distinctTagIds.Length == 0
            ? new List<Tag>()
            : await tags.GetByIdsAsync(kanbanId, distinctTagIds, cancellationToken);

        if (selectedTags.Count != distinctTagIds.Length)
            throw new BadRequestException("One or more tags were not found in kanban");

        await taskTags.ReplaceAsync(taskId, selectedTags.Select(x => x.Id).ToArray(), cancellationToken);
        return selectedTags;
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
