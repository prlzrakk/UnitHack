using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.ChangeKanban;

public class ChangeKanbanHandler(
    IKanbanRepository kanbanRepository,
    ITeamMemberRepository teamMemberRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeKanbanCommand, ChangeKanbanResponse>
{
    public async Task<ChangeKanbanResponse> Handle(ChangeKanbanCommand request, CancellationToken cancellationToken)
    {
        var kanban = await kanbanRepository.GetByIdWithProjectAsync(request.KanbanId, cancellationToken);

        if (kanban is null)
            throw new NotFoundException("Kanban not found");

        var isAdmin =
            await teamMemberRepository.IsAdminAsync(request.KanbanId, request.CurrentUserId, cancellationToken);

        if (!isAdmin)
            throw new ForbiddenException("Only team admin can change kanban");

        kanban.Name = request.Name;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChangeKanbanResponse
        {
            Id = kanban.Id,
            ProjectId = kanban.ProjectId,
            Name = kanban.Name
        };
    }
}