using Microsoft.Extensions.Options;
using RabbitMQ.Client;

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
    }
}