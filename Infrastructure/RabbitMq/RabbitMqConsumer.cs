using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.RabbitMq;

public class RabbitMqConsumer : IRabbitMqConsumer
{
    private readonly RabbitMqOptions _options;

    public RabbitMqConsumer(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }
    
    public async Task ConsumeAsync(string queueName, string routingKey, Func<string, Task> handleMessage, CancellationToken cancellationToken)
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
        await channel.ExchangeDeclareAsync(exchange: "kanban.events", type: ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queue: queueName, exchange: "kanban.events", routingKey: routingKey, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
            try
            {
                await handleMessage(payload);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
            }
            catch
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken: cancellationToken);
            }
        };
        
        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}