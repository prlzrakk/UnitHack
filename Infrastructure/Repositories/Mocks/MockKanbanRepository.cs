using Infrastructure.Constants;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockKanbanRepository(MockDataStore store) : IKanbanRepository
{
    public Task<Kanban> AddAsync(Guid projectId, string name, CancellationToken cancellationToken = default)
    {
        var project = store.Projects.First(x => x.Id == projectId);
        var kanban = new Kanban
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Project = project,
            Name = name.Trim(),
            Columns = [],
            Tags = []
        };

        store.Kanbans.Add(kanban);
        project.Kanbans.Add(kanban);

        var columns = KanbanDefaults.BasicColumns
            .Select(column => new KanbanColumn
            {
                Id = Guid.NewGuid(),
                KanbanId = kanban.Id,
                Kanban = kanban,
                Name = column.Name,
                Order = column.Order,
                Tasks = []
            })
            .ToList();

        kanban.Columns = columns;
        store.KanbanColumns.AddRange(columns);

        return Task.FromResult(kanban);
    }

    public Task<bool> DeleteAsync(
        Guid kanbanId,
        CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans
            .FirstOrDefault(x => x.Id == kanbanId);

        if (kanban is null)
        {
            return Task.FromResult(false);
        }

        store.Kanbans.Remove(kanban);

        var columnIds = store.KanbanColumns
            .Where(x => x.KanbanId == kanbanId)
            .Select(x => x.Id)
            .ToHashSet();

        var taskIds = store.Tasks
            .Where(x => x.KanbanId == kanbanId || columnIds.Contains(x.ColumnId))
            .Select(x => x.Id)
            .ToHashSet();
        var tagIds = store.Tags
            .Where(x => x.KanbanId == kanbanId)
            .Select(x => x.Id)
            .ToHashSet();

        store.TaskTags.RemoveAll(x => taskIds.Contains(x.TaskId) || tagIds.Contains(x.TagId));
        store.Tasks.RemoveAll(x => taskIds.Contains(x.Id));
        store.Tags.RemoveAll(x => x.KanbanId == kanbanId);
        store.KanbanColumns.RemoveAll(x => x.KanbanId == kanbanId);
        kanban.Project?.Kanbans.Remove(kanban);

        return Task.FromResult(true);
    }

    public Task<Kanban?> GetByIdAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans.FirstOrDefault(x => x.Id == kanbanId);
        HydrateKanban(kanban);
        return Task.FromResult(kanban);
    }

    public Task<Kanban?> GetByIdWithProjectAsync(
        Guid kanbanId,
        CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans
            .FirstOrDefault(x => x.Id == kanbanId);

        HydrateKanban(kanban);

        return Task.FromResult(kanban);
    }

    public Task<Kanban?> GetByIdWithProjectAndColumnsAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans
            .FirstOrDefault(x => x.Id == kanbanId);

        if (kanban is null)
            return Task.FromResult<Kanban?>(null);

        if (kanban.Project is null)
        {
            kanban.Project = store.Projects
                .FirstOrDefault(x => x.Id == kanban.ProjectId)!;
        }

        kanban.Columns = store.KanbanColumns
            .Where(x => x.KanbanId == kanban.Id)
            .OrderBy(x => x.Order)
            .ToList();

        return Task.FromResult<Kanban?>(kanban);
    }

    public Task<List<Kanban>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var kanbans = store.Kanbans
            .Where(x => x.ProjectId == projectId)
            .ToList();

        return Task.FromResult(kanbans);
    }

    private void HydrateKanban(Kanban? kanban)
    {
        if (kanban is null)
            return;

        kanban.Project ??= store.Projects.FirstOrDefault(x => x.Id == kanban.ProjectId);
        kanban.Columns = store.KanbanColumns
            .Where(x => x.KanbanId == kanban.Id)
            .OrderBy(x => x.Order)
            .ToList();
        kanban.Tasks = store.Tasks
            .Where(x => x.KanbanId == kanban.Id)
            .OrderBy(x => x.Order)
            .ToList();
        kanban.Tags = store.Tags
            .Where(x => x.KanbanId == kanban.Id)
            .OrderBy(x => x.Name)
            .ToList();

        foreach (var column in kanban.Columns)
        {
            column.Kanban = kanban;
            column.Tasks = store.Tasks
                .Where(x => x.ColumnId == column.Id)
                .OrderBy(x => x.Order)
                .ToList();
        }
    }
}
