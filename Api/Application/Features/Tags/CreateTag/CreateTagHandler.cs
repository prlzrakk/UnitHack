using Api.Application.Common.Exceptions;
using Api.Application.Features.Tags.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Tags.CreateTag;

public class CreateTagHandler(
    IKanbanRepository kanbans,
    ITagRepository tags,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTagCommand, TagResponse>
{
    public async Task<TagResponse> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        var kanban = await kanbans.GetByIdWithProjectAsync(command.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        if (!await members.IsMemberAsync(kanban.Project.TeamId, command.CurrentUserId, cancellationToken))
            throw new ForbiddenException("Only team member can create tags");

        if (await tags.IsNameTakenAsync(kanban.Id, command.Name, null, cancellationToken))
            throw new BadRequestException("Tag name already exists");

        var tag = await tags.AddAsync(kanban.Id, command.Name, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToResponse();
    }
}
