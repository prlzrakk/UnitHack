using Api.Application.Common.Exceptions;
using Api.Application.Features.Tags.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.TaskTags.GetTaskTags;

public class GetTaskTagsHandler(
    IKanbanRepository kanbans,
    IKanbanTaskRepository tasks,
    ITaskTagRepository taskTags,
    ITeamMemberRepository members) : IRequestHandler<GetTaskTagsQuery, List<TagResponse>>
{
    public async Task<List<TagResponse>> Handle(GetTaskTagsQuery query, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(query.TaskId, cancellationToken)
                   ?? throw new NotFoundException("Task not found");
        var kanban = await kanbans.GetByIdWithProjectAsync(task.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        if (!await members.IsMemberAsync(kanban.Project.TeamId, query.CurrentUserId, cancellationToken))
            throw new ForbiddenException("Only team member can view task tags");

        var tags = await taskTags.GetTagsByTaskIdAsync(task.Id, cancellationToken);
        return tags.ToResponseList();
    }
}
