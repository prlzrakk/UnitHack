using MediatR;

namespace Api.Application.Features.Tasks.DeleteTask;

public record DeleteTaskCommand(Guid TaskId, Guid CurrentUserId) : IRequest;
