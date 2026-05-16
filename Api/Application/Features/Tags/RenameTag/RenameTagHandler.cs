using Api.Application.Common.Exceptions;
using Api.Application.Features.Tags.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tags.RenameTag;

public class RenameTagHandler(
    IKanbanRepository kanbans,
    ITagRepository tags,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<RenameTagCommand, TagResponse>
{
    public async Task<TagResponse> Handle(RenameTagCommand command, CancellationToken cancellationToken)
    {
        var tag = await tags.GetByIdAsync(command.TagId, cancellationToken)
                  ?? throw new NotFoundException("Tag not found");
        var kanban = await kanbans.GetByIdWithProjectAsync(tag.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        if (!await members.IsMemberAsync(kanban.Project.TeamId, command.CurrentUserId, cancellationToken))
            throw new ForbiddenException("Only team member can rename tags");

        if (await tags.IsNameTakenAsync(kanban.Id, command.Name, tag.Id, cancellationToken))
            throw new BadRequestException("Tag name already exists");

        var renamedTag = await tags.RenameAsync(tag.Id, command.Name, cancellationToken)
                         ?? throw new NotFoundException("Tag not found");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return renamedTag.ToResponse();
    }
}
