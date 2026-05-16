using Api.Application.Features.Tags.Common;
using MediatR;

namespace Api.Application.Features.TaskTags.GetTaskTags;

public record GetTaskTagsQuery(Guid TaskId, Guid CurrentUserId) : IRequest<List<TagResponse>>;
