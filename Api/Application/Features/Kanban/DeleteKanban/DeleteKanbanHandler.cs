using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.DeleteKanban;

public class DeleteKanbanHandler(IKanbanRepository kanbanRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteKanbanCommand>
{
    public async Task Handle(DeleteKanbanCommand request, CancellationToken cancellationToken)
    {
        var deleted = await kanbanRepository.DeleteAsync(request.KanbanId, cancellationToken);

        if (!deleted)
            throw new NotFoundException("Kanban not found");

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}