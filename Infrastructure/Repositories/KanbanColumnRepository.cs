using Infrastructure.Constants;
using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class KanbanColumnRepository(DatabaseContext context) : IKanbanColumnRepository
{
    public async Task<KanbanColumn> AddAsync(
        Guid kanbanId,
        string name,
        int? order,
        CancellationToken cancellationToken)
    {
        var columnOrder = order ?? await GetNextOrderAsync(kanbanId, cancellationToken);
        var column = new KanbanColumn
        {
            Id = Guid.NewGuid(),
            KanbanId = kanbanId,
            Name = name.Trim(),
            Order = columnOrder,
            Tasks = []
        };

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

    private async Task<int> GetNextOrderAsync(Guid kanbanId, CancellationToken cancellationToken)
    {
        var maxOrder = await context.KanbanColumns
            .Where(x => x.KanbanId == kanbanId)
            .Select(x => (int?)x.Order)
            .MaxAsync(cancellationToken);

        return (maxOrder ?? 0) + KanbanDefaults.OrderStep;
    }
}
