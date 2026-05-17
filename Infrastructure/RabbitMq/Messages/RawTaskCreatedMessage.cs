namespace Infrastructure.RabbitMq.Messages;

public sealed record RawTaskCreatedMessage(
    Guid MessageId,
    Guid ProjectId,
    Guid CreatedByUserId,
    string Title,
    string? Description,
    string Priority,
    string[] Tags,
    DateTime? Deadline,
    DateTime CreatedAt
);