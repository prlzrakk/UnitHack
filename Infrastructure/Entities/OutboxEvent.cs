using Infrastructure.Enums;

namespace Infrastructure.Entities;

public class OutboxEvent
{
    public Guid Id { get; set; }

    public EventType EventType { get; set; }

    public string Payload { get; set; } = null!;

    public string Status { get; set; } = "Pending";

    public int RetryCount { get; set; }

    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAt { get; set; }

}