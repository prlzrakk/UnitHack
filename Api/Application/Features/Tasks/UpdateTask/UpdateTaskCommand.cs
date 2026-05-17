using Api.Application.Features.Tasks.Common;
using Infrastructure.Enums;
using MediatR;

namespace Api.Application.Features.Tasks.UpdateTask;

public record UpdateTaskCommand(
    Guid TaskId,
    Guid CurrentUserId,
    string Name,
    string? Description,
    Priority Priority,
    DateTime Deadline,
    Guid UserId,
    Guid[]? TagIds) : IRequest<TaskResponse>;
