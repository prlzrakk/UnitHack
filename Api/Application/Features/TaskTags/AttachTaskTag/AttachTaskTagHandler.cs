using Api.Application.Common.Exceptions;
using Api.Application.Features.Tags.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.TaskTags.AttachTaskTag;

public class AttachTaskTagHandler(
    IKanbanRepository kanbans,
    IKanbanTaskRepository tasks,
    ITagRepository tags,
    ITaskTagRepository taskTags,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<AttachTaskTagCommand, List<TagResponse>>
{
    public async Task<List<TagResponse>> Handle(AttachTaskTagCommand command, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(command.TaskId, cancellationToken)
                   ?? throw new NotFoundException("Task not found");
        var tag = await tags.GetByIdAsync(command.TagId, cancellationToken)
                  ?? throw new NotFoundException("Tag not found");

        if (tag.KanbanId != task.KanbanId)
            throw new BadRequestException("Tag does not belong to task kanban");

        var kanban = await kanbans.GetByIdWithProjectAsync(task.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        if (!await members.IsMemberAsync(kanban.Project.TeamId, command.CurrentUserId, cancellationToken))
            throw new ForbiddenException("Only team member can attach task tags");

        await taskTags.AttachAsync(task.Id, tag.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var currentTags = await taskTags.GetTagsByTaskIdAsync(task.Id, cancellationToken);
        return currentTags.ToResponseList();
    }
}
