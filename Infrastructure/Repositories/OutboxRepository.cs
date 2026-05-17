using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OutboxRepository(DatabaseContext context) : IOutboxRepository
{
    public async Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken)
    {
        await context.OutboxEvents.AddAsync(outboxEvent, cancellationToken);
    }

    public async Task<List<OutboxEvent>> GetPendingAsync(CancellationToken cancellationToken)
    {
        return await context.OutboxEvents
            .Where(x => x.Status == "Pending")
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken)
    {
        context.OutboxEvents.Update(outboxEvent);
        return Task.CompletedTask;
    }
}