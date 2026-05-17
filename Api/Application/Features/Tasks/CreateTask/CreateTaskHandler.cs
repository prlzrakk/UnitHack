using Api.Application.Common.Exceptions;
using Api.Application.Features.Tags.Common;
using Api.Application.Features.Tasks.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tasks.CreateTask;

public class CreateTaskHandler(
    IKanbanRepository kanbans,
    IKanbanColumnRepository columns,
    IKanbanTaskRepository tasks,
    ITagRepository tags,
    ITaskTagRepository taskTags,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTaskCommand, TaskResponse>
{
    public async Task<TaskResponse> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var kanban = await kanbans.GetByIdWithProjectAsync(command.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");
        var column = await columns.GetByIdAsync(command.ColumnId, cancellationToken)
                     ?? throw new NotFoundException("Column not found");

        if (column.KanbanId != kanban.Id)
            throw new BadRequestException("Column does not belong to kanban");

        var teamId = kanban.Project.TeamId;
        if (!await members.IsMemberAsync(teamId, command.CurrentUserId, cancellationToken))
            throw new ForbiddenException("Only team member can create tasks");
        if (!await members.IsMemberAsync(teamId, command.UserId, cancellationToken))
            throw new BadRequestException("Assignee is not a team member");

        var selectedTags = await GetValidatedTagsAsync(kanban.Id, command.TagIds, cancellationToken);
        var task = await tasks.AddAsync(
            kanban.Id,
            column.Id,
            command.UserId,
            command.Name,
            command.Description,
            command.Priority,
            command.Deadline,
            command.Order,
            cancellationToken);

        await taskTags.ReplaceAsync(task.Id, selectedTags.Select(x => x.Id).ToArray(), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(task, selectedTags);
    }

    private async Task<List<Tag>> GetValidatedTagsAsync(
        Guid kanbanId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        var distinctTagIds = tagIds.Distinct().ToArray();
        if (distinctTagIds.Length == 0)
            return [];

        var kanbanTags = await tags.GetByIdsAsync(kanbanId, distinctTagIds, cancellationToken);
        if (kanbanTags.Count != distinctTagIds.Length)
            throw new BadRequestException("One or more tags were not found in kanban");

        return kanbanTags;
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
