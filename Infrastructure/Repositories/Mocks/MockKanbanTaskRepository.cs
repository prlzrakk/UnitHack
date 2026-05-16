using Infrastructure.Constants;
using Infrastructure.Entities;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockKanbanTaskRepository(MockDataStore store) : IKanbanTaskRepository
{
    public Task<KanbanTask> AddAsync(
        Guid kanbanId,
        Guid columnId,
        Guid userId,
        string name,
        string? description,
        Priority priority,
        DateTime deadline,
        int? order,
        CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans.First(x => x.Id == kanbanId);
        var column = store.KanbanColumns.First(x => x.Id == columnId);
        var task = new KanbanTask
        {
            Id = Guid.NewGuid(),
            KanbanId = kanban.Id,
            Kanban = kanban,
            ColumnId = column.Id,
            Column = column,
            UserId = userId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Priority = priority,
            Deadline = deadline,
            CreatedAt = DateTime.UtcNow,
            Order = order ?? GetNextOrder(kanban.Id)
        };

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

        var taskTags = store.TaskTags
            .Where(x => x.TaskId == taskId)
            .ToList();

        foreach (var taskTag in taskTags)
        {
            store.TaskTags.Remove(taskTag);
            taskTag.Tag?.TaskTags.Remove(taskTag);
            task.TaskTags.Remove(taskTag);
        }

        store.Tasks.Remove(task);
        task.Kanban?.Tasks.Remove(task);
        task.Column?.Tasks.Remove(task);

        return Task.FromResult(true);
    }

    private int GetNextOrder(Guid kanbanId)
    {
        var maxOrder = store.Tasks
            .Where(x => x.KanbanId == kanbanId)
            .Select(x => (int?)x.Order)
            .Max();

        return (maxOrder ?? 0) + KanbanDefaults.OrderStep;
    }
}
