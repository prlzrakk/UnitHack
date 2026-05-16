using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tags.DeleteTag;

public class DeleteTagHandler(
    IKanbanRepository kanbans,
    ITagRepository tags,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteTagCommand>
{
    public async Task Handle(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        var tag = await tags.GetByIdAsync(command.TagId, cancellationToken)
                  ?? throw new NotFoundException("Tag not found");
        var kanban = await kanbans.GetByIdWithProjectAsync(tag.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        if (!await members.IsMemberAsync(kanban.Project.TeamId, command.CurrentUserId, cancellationToken))
            throw new ForbiddenException("Only team member can delete tags");

        await tags.DeleteAsync(tag.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
