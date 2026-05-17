using Infrastructure.RabbitMq;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Workers;

public class OutboxWorker(
    IServiceScopeFactory scopeFactory,
    IRabbitMqPublisher publisher,
    ILogger<OutboxWorker> logger) : BackgroundService
{
    private const int PollingDelayMilliseconds = 1000;
    private const int ErrorDelayMilliseconds = 5000;
    private const int MaxPublishAttempts = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
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
                        outboxEvent.LastError = null;
                        await repository.UpdateAsync(outboxEvent, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        outboxEvent.RetryCount++;
                        outboxEvent.LastError = exception.Message;
                        outboxEvent.Status = outboxEvent.RetryCount >= MaxPublishAttempts
                            ? "Failed"
                            : "Pending";

                        logger.LogError(
                            exception,
                            "Failed to publish outbox event {OutboxEventId}. Attempt {Attempt}/{MaxAttempts}.",
                            outboxEvent.Id,
                            outboxEvent.RetryCount,
                            MaxPublishAttempts);

                        await repository.UpdateAsync(outboxEvent, stoppingToken);
                    }
                }

                await unitOfWork.SaveChangesAsync(stoppingToken);
                await Task.Delay(PollingDelayMilliseconds, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox worker failed. It will retry in 5 seconds.");
                await Task.Delay(ErrorDelayMilliseconds, stoppingToken);
            }
        }
    }
}
