namespace Infrastructure.RabbitMq;

public interface IRabbitMqConsumer
{
    Task ConsumeAsync(string queueName, string routingKey, Func<string, string, Task> handleMessage, CancellationToken cancellationToken);
}