using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.DeleteKanban;

public class DeleteKanbanHandler(
    IKanbanRepository kanbanRepository,
    ITeamMemberRepository teamMemberRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteKanbanCommand>
{
    public async Task Handle(DeleteKanbanCommand request, CancellationToken cancellationToken)
    {
        var kanban = await kanbanRepository.GetByIdWithProjectAsync(request.KanbanId, cancellationToken);
        if (kanban is null)
            throw new NotFoundException("Kanban not found");

        var isAdmin = await teamMemberRepository.IsAdminAsync(
            kanban.Project.TeamId,
            request.CurrentUserId,
            cancellationToken);

        if (!isAdmin)
            throw new ForbiddenException("Only team admin can delete kanban");

        await kanbanRepository.DeleteAsync(kanban.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
