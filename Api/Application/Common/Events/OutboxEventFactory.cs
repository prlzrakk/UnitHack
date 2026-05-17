using System.Text.Json;
using Infrastructure.Entities;
using Infrastructure.Enums;

namespace Api.Application.Common.Events;

public class OutboxEventFactory
{
    public static OutboxEvent Create(EventType eventType, object payload)
    {
        var now = DateTime.UtcNow;
        return new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = JsonSerializer.Serialize(payload),
            Status = "Pending",
            RetryCount = 0,
            CreatedAt = now,
        };
    }
}