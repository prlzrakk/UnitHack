using Infrastructure.Enums;

namespace Api.Application.Features.Tasks.UpdateTask;

public sealed class UpdateTaskRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Priority Priority { get; init; }
    public DateTime Deadline { get; init; }
    public Guid UserId { get; init; }
}
