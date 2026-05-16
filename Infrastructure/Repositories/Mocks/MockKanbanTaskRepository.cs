using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockKanbanTaskRepository(MockDataStore store) : IKanbanTaskRepository
{
    public Task<KanbanTask> AddAsync(KanbanTask task, CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans.First(x => x.Id == task.KanbanId);
        var column = store.KanbanColumns.First(x => x.Id == task.ColumnId);

        task.Kanban = kanban;
        task.Column = column;

        store.Tasks.Add(task);
        kanban.Tasks.Add(task);
        column.Tasks ??= [];
        column.Tasks.Add(task);

        return Task.FromResult(task);
    }

    public Task<KanbanTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = store.Tasks.FirstOrDefault(x => x.Id == taskId);
        if (task is not null)
        {
            task.Kanban ??= store.Kanbans.FirstOrDefault(x => x.Id == task.KanbanId);
            task.Column ??= store.KanbanColumns.FirstOrDefault(x => x.Id == task.ColumnId);
        }

        return Task.FromResult(task);
    }

    public Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = store.Tasks.FirstOrDefault(x => x.Id == taskId);
        if (task is null)
            return Task.FromResult(false);

        store.Tasks.Remove(task);
        task.Kanban?.Tasks.Remove(task);
        task.Column?.Tasks.Remove(task);

        return Task.FromResult(true);
    }
}
