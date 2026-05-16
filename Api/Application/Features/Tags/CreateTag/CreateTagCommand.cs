using Api.Application.Features.Tags.Common;
using MediatR;

namespace Api.Application.Features.Tags.CreateTag;

public record CreateTagCommand(Guid KanbanId, Guid CurrentUserId, string Name) : IRequest<TagResponse>;
