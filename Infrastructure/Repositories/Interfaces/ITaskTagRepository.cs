using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface ITaskTagRepository
{
    Task<List<Tag>> GetTagsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<TaskTag> AttachAsync(Guid taskId, Guid tagId, CancellationToken cancellationToken);
    Task<bool> DetachAsync(Guid taskId, Guid tagId, CancellationToken cancellationToken);
    Task ReplaceAsync(Guid taskId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken);
}
