using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IKanbanColumnRepository
{
    Task<KanbanColumn> AddAsync(Guid kanbanId, string name, int? order, CancellationToken cancellationToken);
    Task<KanbanColumn?> GetByIdAsync(Guid columnId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid columnId, CancellationToken cancellationToken);
}
