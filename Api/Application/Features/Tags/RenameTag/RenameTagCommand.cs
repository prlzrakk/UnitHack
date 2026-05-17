using Api.Application.Features.Tags.Common;
using MediatR;

namespace Api.Application.Features.Tags.RenameTag;

public record RenameTagCommand(Guid TagId, Guid CurrentUserId, string Name) : IRequest<TagResponse>;
