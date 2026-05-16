using Api.Application.Features.Tasks.Common;
using MediatR;

namespace Api.Application.Features.Tasks.MoveTask;

public record MoveTaskCommand(Guid TaskId, Guid CurrentUserId, Guid ToColumnId, int Order) : IRequest<TaskResponse>;
