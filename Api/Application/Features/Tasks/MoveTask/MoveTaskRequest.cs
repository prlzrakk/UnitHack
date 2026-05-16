namespace Api.Application.Features.Tasks.MoveTask;

public sealed class MoveTaskRequest
{
    public Guid ToColumnId { get; init; }
    public int Order { get; init; }
}
