using MediatR;

namespace Api.Application.Features.TaskTags.DetachTaskTag;

public record DetachTaskTagCommand(Guid TaskId, Guid TagId, Guid CurrentUserId) : IRequest;
