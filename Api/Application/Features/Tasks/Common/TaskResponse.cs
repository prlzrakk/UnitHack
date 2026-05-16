using Infrastructure.Enums;

namespace Api.Application.Features.Tasks.Common;

public record TaskResponse(
    Guid Id,
    Guid KanbanId,
    Guid ColumnId,
    Guid UserId,
    string Name,
    string? Description,
    Priority Priority,
    DateTime CreatedAt,
    DateTime Deadline,
    int Order);
