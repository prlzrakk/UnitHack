using MediatR;

namespace Api.Application.Features.Tags.DeleteTag;

public record DeleteTagCommand(Guid TagId, Guid CurrentUserId) : IRequest;
