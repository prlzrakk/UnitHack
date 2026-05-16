using Api.Application.Features.Tags.Common;
using MediatR;

namespace Api.Application.Features.Tags.GetKanbanTags;

public record GetKanbanTagsQuery(Guid KanbanId, Guid CurrentUserId) : IRequest<List<TagResponse>>;
