using Api.Application.Features.Tasks.Common;
using Infrastructure.Enums;
using MediatR;

namespace Api.Application.Features.Tasks.CreateTask;

public record CreateTaskCommand(
    Guid KanbanId,
    Guid CurrentUserId,
    string Name,
    string? Description,
    Priority Priority,
    DateTime Deadline,
    Guid UserId,
    Guid ColumnId,
    int? Order) : IRequest<TaskResponse>;
