using Infrastructure.Repositories.Interfaces;
using MediatR;
using WebApplication1.Application.Common.Exceptions;

namespace WebApplication1.Application.Features.Kanban.DeleteKanban;

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