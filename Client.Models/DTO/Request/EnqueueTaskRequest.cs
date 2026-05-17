namespace Client.Models.DTO.Request;

public sealed record EnqueueTaskRequest(
    Guid ProjectId,
    Guid CreatedByUserId,
    string Title,
    string? Description,
    string? Priority,
    string[]? Tags,
    DateTime? Deadline
);