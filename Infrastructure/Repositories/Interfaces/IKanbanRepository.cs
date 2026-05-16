using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IKanbanRepository
{
    Task AddAsync(Kanban kanban, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid kanbanId, CancellationToken cancellationToken);
    Task<Kanban?> GetByIdWithProjectAsync(Guid kanbanId, CancellationToken cancellationToken);
    Task<List<Kanban>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken);
}