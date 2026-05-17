using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TagRepository(DatabaseContext context) : ITagRepository
{
    public async Task<Tag> AddAsync(Guid kanbanId, string name, CancellationToken cancellationToken)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            KanbanId = kanbanId,
            Name = name.Trim()
        };

        await context.Tags.AddAsync(tag, cancellationToken);
        return tag;
    }

    public async Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken)
    {
        return await context.Tags
            .FirstOrDefaultAsync(x => x.Id == tagId, cancellationToken);
    }

    public async Task<List<Tag>> GetByKanbanIdAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        return await context.Tags
            .Where(x => x.KanbanId == kanbanId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Tag>> GetByIdsAsync(
        Guid kanbanId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        return await context.Tags
            .Where(x => x.KanbanId == kanbanId && tagIds.Contains(x.Id))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsNameTakenAsync(
        Guid kanbanId,
        string name,
        Guid? excludedTagId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();

        return await context.Tags
            .AnyAsync(x =>
                x.KanbanId == kanbanId &&
                x.Name == normalizedName &&
                (!excludedTagId.HasValue || x.Id != excludedTagId.Value),
                cancellationToken);
    }

    public async Task<Tag?> RenameAsync(Guid tagId, string name, CancellationToken cancellationToken)
    {
        var tag = await context.Tags
            .FirstOrDefaultAsync(x => x.Id == tagId, cancellationToken);

        if (tag is null)
            return null;

        tag.Name = name.Trim();
        return tag;
    }

    public async Task<bool> DeleteAsync(Guid tagId, CancellationToken cancellationToken)
    {
        var tag = await context.Tags
            .FirstOrDefaultAsync(x => x.Id == tagId, cancellationToken);

        if (tag is null)
            return false;

        context.Tags.Remove(tag);
        return true;
    }
}
