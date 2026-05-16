using Infrastructure.Enums;

namespace Api.Application.Features.Tasks.CreateTask;

public sealed class CreateTaskRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Priority Priority { get; init; }
    public DateTime Deadline { get; init; }
    public Guid UserId { get; init; }
    public Guid ColumnId { get; init; }
    public int? Order { get; init; }
    public Guid[] TagIds { get; init; } = [];
}
