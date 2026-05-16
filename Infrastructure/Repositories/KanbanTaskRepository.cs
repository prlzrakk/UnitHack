using Infrastructure.Constants;
using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class KanbanTaskRepository(DatabaseContext context) : IKanbanTaskRepository
{
    public async Task<KanbanTask> AddAsync(
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
        var taskOrder = order ?? await GetNextOrderAsync(kanbanId, cancellationToken);
        var task = new KanbanTask
        {
            Id = Guid.NewGuid(),
            KanbanId = kanbanId,
            ColumnId = columnId,
            UserId = userId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Priority = priority,
            Deadline = deadline,
            CreatedAt = DateTime.UtcNow,
            Order = taskOrder
        };

        await context.Tasks.AddAsync(task, cancellationToken);
        return task;
    }

    public async Task<KanbanTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken)
    {
        return await context.Tasks
            .Include(x => x.Column)
            .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await context.Tasks
            .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);

        if (task is null)
            return false;

        context.Tasks.Remove(task);
        return true;
    }

    private async Task<int> GetNextOrderAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        var maxOrder = await context.Tasks
            .Where(x => x.KanbanId == kanbanId)
            .Select(x => (int?)x.Order)
            .MaxAsync(cancellationToken);

        return (maxOrder ?? 0) + KanbanDefaults.OrderStep;
    }
}
