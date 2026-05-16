using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface ITagRepository
{
    Task<Tag> AddAsync(Guid kanbanId, string name, CancellationToken cancellationToken);
    Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken);
    Task<List<Tag>> GetByKanbanIdAsync(Guid kanbanId, CancellationToken cancellationToken);
    Task<List<Tag>> GetByIdsAsync(Guid kanbanId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken);
    Task<bool> IsNameTakenAsync(Guid kanbanId, string name, Guid? excludedTagId, CancellationToken cancellationToken);
    Task<Tag?> RenameAsync(Guid tagId, string name, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid tagId, CancellationToken cancellationToken);
}
