using Api.Application.Features.Tags.Common;
using MediatR;

namespace Api.Application.Features.TaskTags.AttachTaskTag;

public record AttachTaskTagCommand(Guid TaskId, Guid TagId, Guid CurrentUserId) : IRequest<List<TagResponse>>;
