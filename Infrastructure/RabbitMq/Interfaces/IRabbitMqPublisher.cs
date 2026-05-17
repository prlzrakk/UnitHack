namespace Infrastructure.RabbitMq;

public interface IRabbitMqPublisher
{
    // routingKey - в какую очередь отправлять, payload - само сообщение в json
    Task PublishAsync(string routingKey, string payload, CancellationToken cancellationToken);
}