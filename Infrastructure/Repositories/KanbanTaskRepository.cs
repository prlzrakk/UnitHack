using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class KanbanTaskRepository(DatabaseContext context) : IKanbanTaskRepository
{
    public async Task<KanbanTask> AddAsync(KanbanTask task, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(task);
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
}
