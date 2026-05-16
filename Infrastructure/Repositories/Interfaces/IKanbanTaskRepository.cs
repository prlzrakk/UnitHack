using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IKanbanTaskRepository
{
    Task<KanbanTask> AddAsync(KanbanTask task, CancellationToken cancellationToken);
    Task<KanbanTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken);
}
