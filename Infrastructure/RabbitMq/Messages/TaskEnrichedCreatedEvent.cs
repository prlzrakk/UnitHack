namespace Infrastructure.RabbitMq.Messages;

public sealed record TaskEnrichedCreatedEvent(
    Guid MessageId,
    Guid TaskId,
    Guid ProjectId,
    string ProjectName,
    Guid CreatedByUserId,
    string CreatedByUserName,
    Guid ColumnId,
    string ColumnName,
    string Title,
    string? Description,
    string Priority,
    string[] Tags,
    DateTime CreatedAt,
    DateTime? Deadline
);