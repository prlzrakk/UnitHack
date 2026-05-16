using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IKanbanRepository
{
    void Add(Kanban kanban);
    Task<bool> DeleteAsync(Guid kanbanId, CancellationToken cancellationToken);
    Task<Kanban?> GetByIdWithProjectAsync(Guid kanbanId, CancellationToken cancellationToken);
    Task<List<Kanban>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken);
}