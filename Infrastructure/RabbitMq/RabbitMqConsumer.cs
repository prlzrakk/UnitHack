using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.RabbitMq;

public class RabbitMqConsumer(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConsumer> logger) : IRabbitMqConsumer
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task ConsumeAsync(string queueName, string routingKey, Func<string, string, Task> handleMessage, CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                UserName = _options.Username,
                Password = _options.Password,
                Port = _options.Port,
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await channel.ExchangeDeclareAsync(exchange: _options.Exchange, type: ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(queue: queueName, exchange: _options.Exchange, routingKey: routingKey, cancellationToken: cancellationToken);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                var eventType = ea.RoutingKey;
                var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    await handleMessage(eventType, payload);
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Failed to handle RabbitMQ message from queue {QueueName} with routing key {RoutingKey}. The message will not be requeued.",
                        queueName,
                        eventType);

                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
