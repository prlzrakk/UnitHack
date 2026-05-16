using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class KanbanRepository(DatabaseContext context) : IKanbanRepository
{
    public async Task AddAsync(Kanban kanban, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kanban);
        await context.Kanbans.AddAsync(kanban, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        var kanban = await context.Kanbans
            .FirstOrDefaultAsync(x => x.Id == kanbanId, cancellationToken);

        if (kanban is null)
            return false;

        context.Kanbans.Remove(kanban);
        return true;
    }

    public async Task<Kanban?> GetByIdAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        return await context.Kanbans
            .FirstOrDefaultAsync(x => x.Id == kanbanId, cancellationToken);
    }

    public async Task<Kanban?> GetByIdWithProjectAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        return await context.Kanbans
            .Include(k => k.Project)
            .Include(k => k.Columns)
            .Include(k => k.Tasks)
            .FirstOrDefaultAsync(x => x.Id == kanbanId, cancellationToken);
    }

    public async Task<Kanban?> GetByIdWithProjectAndColumnsAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        return await context.Kanbans
            .Include(k => k.Project)
            .Include(k => k.Columns)
            .FirstOrDefaultAsync(x => x.Id == kanbanId, cancellationToken);
    }

    public async Task<List<Kanban>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await context.Kanbans
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }
}
