using Infrastructure.RabbitMq;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Workers;

public class OutboxWorker (IServiceScopeFactory scopeFactory, IRabbitMqPublisher publisher) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var events = await repository.GetPendingAsync(stoppingToken);

            foreach (var outboxEvent in events)
            {
                try
                {
                    await publisher.PublishAsync(
                        routingKey: outboxEvent.EventType.ToString(),
                        payload: outboxEvent.Payload,
                        cancellationToken: stoppingToken
                    );
                    outboxEvent.Status = "Published";
                    outboxEvent.PublishedAt = DateTime.UtcNow;
                    await repository.UpdateAsync(outboxEvent, stoppingToken);
                }
                catch
                {
                    outboxEvent.Status = "Failed";
                    await repository.UpdateAsync(outboxEvent, stoppingToken);
                }
            }
            
            await unitOfWork.SaveChangesAsync(stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }
}