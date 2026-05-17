using Api.Application.Common.Exceptions;
using Api.Application.Features.Tags.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tags.GetKanbanTags;

public class GetKanbanTagsHandler(
    IKanbanRepository kanbans,
    ITagRepository tags,
    ITeamMemberRepository members) : IRequestHandler<GetKanbanTagsQuery, List<TagResponse>>
{
    public async Task<List<TagResponse>> Handle(GetKanbanTagsQuery query, CancellationToken cancellationToken)
    {
        var kanban = await kanbans.GetByIdWithProjectAsync(query.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        if (!await members.IsMemberAsync(kanban.Project.TeamId, query.CurrentUserId, cancellationToken))
            throw new ForbiddenException("Only team member can view tags");

        var kanbanTags = await tags.GetByKanbanIdAsync(kanban.Id, cancellationToken);
        return kanbanTags.ToResponseList();
    }
}
