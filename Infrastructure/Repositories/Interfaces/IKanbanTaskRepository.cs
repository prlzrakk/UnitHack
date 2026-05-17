using Infrastructure.Entities;
using Infrastructure.Enums;

namespace Infrastructure.Repositories.Interfaces;

public interface IKanbanTaskRepository
{
    Task<KanbanTask> AddAsync(
        Guid kanbanId,
        Guid columnId,
        Guid userId,
        string name,
        string? description,
        Priority priority,
        DateTime deadline,
        int? order,
        CancellationToken cancellationToken);

    Task<KanbanTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken);
}