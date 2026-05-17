using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken);
    Task<List<OutboxEvent>> GetPendingAsync(CancellationToken cancellationToken);
    Task UpdateAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken);
}