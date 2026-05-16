using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IKanbanColumnRepository
{
    Task<KanbanColumn> AddAsync(KanbanColumn column, CancellationToken cancellationToken);
    Task<KanbanColumn?> GetByIdAsync(Guid columnId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid columnId, CancellationToken cancellationToken);
}
