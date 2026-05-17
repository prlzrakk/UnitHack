using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IKanbanRepository
{
    Task<Kanban> AddAsync(Guid projectId, string name, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid kanbanId, CancellationToken cancellationToken);
    Task<Kanban?> GetByIdAsync(Guid kanbanId, CancellationToken cancellationToken);
    Task<Kanban?> GetByIdWithProjectAsync(Guid kanbanId, CancellationToken cancellationToken);

    Task<Kanban?> GetByIdWithProjectAndColumnsAsync(Guid kanbanId, CancellationToken cancellationToken);

    Task<List<Kanban>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken);
}
