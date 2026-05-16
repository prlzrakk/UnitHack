using Infrastructure.Constants;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockKanbanColumnRepository(MockDataStore store) : IKanbanColumnRepository
{
    public Task<KanbanColumn> AddAsync(
        Guid kanbanId,
        string name,
        int? order,
        CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans.First(x => x.Id == kanbanId);
        var column = new KanbanColumn
        {
            Id = Guid.NewGuid(),
            KanbanId = kanban.Id,
            Kanban = kanban,
            Name = name.Trim(),
            Order = order ?? GetNextOrder(kanban.Id),
            Tasks = []
        };

        store.KanbanColumns.Add(column);
        kanban.Columns.Add(column);

        return Task.FromResult(column);
    }

    public Task<KanbanColumn?> GetByIdAsync(Guid columnId, CancellationToken cancellationToken)
    {
        var column = store.KanbanColumns.FirstOrDefault(x => x.Id == columnId);
        if (column is not null)
        {
            column.Kanban ??= store.Kanbans.FirstOrDefault(x => x.Id == column.KanbanId);
            column.Tasks = store.Tasks
                .Where(x => x.ColumnId == column.Id)
                .OrderBy(x => x.Order)
                .ToList();
        }

        return Task.FromResult(column);
    }

    public Task<bool> DeleteAsync(Guid columnId, CancellationToken cancellationToken)
    {
        var column = store.KanbanColumns.FirstOrDefault(x => x.Id == columnId);
        if (column is null)
            return Task.FromResult(false);

        var taskIds = store.Tasks
            .Where(x => x.ColumnId == columnId)
            .Select(x => x.Id)
            .ToHashSet();

        store.TaskTags.RemoveAll(x => taskIds.Contains(x.TaskId));
        store.Tasks.RemoveAll(x => taskIds.Contains(x.Id));
        store.KanbanColumns.Remove(column);
        column.Kanban?.Columns.Remove(column);

        return Task.FromResult(true);
    }

    private int GetNextOrder(Guid kanbanId)
    {
        var maxOrder = store.KanbanColumns
            .Where(x => x.KanbanId == kanbanId)
            .Select(x => (int?)x.Order)
            .Max();

        return (maxOrder ?? 0) + KanbanDefaults.OrderStep;
    }
}
