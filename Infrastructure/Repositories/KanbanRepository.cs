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
        var kanban = await context.Kanbans.FindAsync(kanbanId, cancellationToken);
        if (kanban == null)
            return false;
        context.Kanbans.Remove(kanban);
        return true;
    }

    public async Task<Kanban?> GetByIdWithProjectAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        var kanban = await context.Kanbans
            .Include(k => k.Project)
            .FirstOrDefaultAsync(x => x.Id == kanbanId, cancellationToken);
        return kanban;
    }

    public async Task<List<Kanban>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await context.Kanbans
            .Where(x => x.Project.Id == projectId)
            .ToListAsync(cancellationToken);
        return project;
    }
}