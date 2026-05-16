using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockKanbanColumnRepository(MockDataStore store) : IKanbanColumnRepository
{
    public Task<KanbanColumn> AddAsync(KanbanColumn column, CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans.First(x => x.Id == column.KanbanId);
        column.Kanban = kanban;
        column.Tasks ??= [];

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

        store.Tasks.RemoveAll(x => x.ColumnId == columnId);
        store.KanbanColumns.Remove(column);
        column.Kanban?.Columns.Remove(column);

        return Task.FromResult(true);
    }
}
