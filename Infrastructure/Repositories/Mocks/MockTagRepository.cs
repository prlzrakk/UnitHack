using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockTagRepository(MockDataStore store) : ITagRepository
{
    public Task<Tag> AddAsync(Guid kanbanId, string name, CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans.First(x => x.Id == kanbanId);
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            KanbanId = kanban.Id,
            Kanban = kanban,
            Name = name.Trim()
        };

        store.Tags.Add(tag);
        kanban.Tags.Add(tag);

        return Task.FromResult(tag);
    }

    public Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken)
    {
        var tag = store.Tags.FirstOrDefault(x => x.Id == tagId);
        if (tag is not null)
            tag.Kanban ??= store.Kanbans.FirstOrDefault(x => x.Id == tag.KanbanId);

        return Task.FromResult(tag);
    }

    public Task<List<Tag>> GetByKanbanIdAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        var tags = store.Tags
            .Where(x => x.KanbanId == kanbanId)
            .OrderBy(x => x.Name)
            .ToList();

        return Task.FromResult(tags);
    }

    public Task<List<Tag>> GetByIdsAsync(
        Guid kanbanId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        var tags = store.Tags
            .Where(x => x.KanbanId == kanbanId && tagIds.Contains(x.Id))
            .OrderBy(x => x.Name)
            .ToList();

        return Task.FromResult(tags);
    }

    public Task<bool> IsNameTakenAsync(
        Guid kanbanId,
        string name,
        Guid? excludedTagId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();
        var isTaken = store.Tags.Any(x =>
            x.KanbanId == kanbanId &&
            x.Name == normalizedName &&
            (!excludedTagId.HasValue || x.Id != excludedTagId.Value));

        return Task.FromResult(isTaken);
    }

    public Task<Tag?> RenameAsync(Guid tagId, string name, CancellationToken cancellationToken)
    {
        var tag = store.Tags.FirstOrDefault(x => x.Id == tagId);
        if (tag is null)
            return Task.FromResult<Tag?>(null);

        tag.Name = name.Trim();
        return Task.FromResult<Tag?>(tag);
    }

    public Task<bool> DeleteAsync(Guid tagId, CancellationToken cancellationToken)
    {
        var tag = store.Tags.FirstOrDefault(x => x.Id == tagId);
        if (tag is null)
            return Task.FromResult(false);

        var taskTags = store.TaskTags
            .Where(x => x.TagId == tagId)
            .ToList();

        foreach (var taskTag in taskTags)
        {
            store.TaskTags.Remove(taskTag);
            taskTag.Task?.TaskTags.Remove(taskTag);
            tag.TaskTags.Remove(taskTag);
        }

        store.Tags.Remove(tag);
        tag.Kanban?.Tags.Remove(tag);

        return Task.FromResult(true);
    }
}
