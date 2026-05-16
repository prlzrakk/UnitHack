using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class KanbanColumnRepository(DatabaseContext context) : IKanbanColumnRepository
{
    public async Task<KanbanColumn> AddAsync(KanbanColumn column, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(column);
        await context.KanbanColumns.AddAsync(column, cancellationToken);
        return column;
    }

    public async Task<KanbanColumn?> GetByIdAsync(Guid columnId, CancellationToken cancellationToken)
    {
        return await context.KanbanColumns
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == columnId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid columnId, CancellationToken cancellationToken)
    {
        var column = await context.KanbanColumns
            .FirstOrDefaultAsync(x => x.Id == columnId, cancellationToken);

        if (column is null)
            return false;

        context.KanbanColumns.Remove(column);
        return true;
    }
}
