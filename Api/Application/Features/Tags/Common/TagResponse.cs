using Infrastructure.Entities;

namespace Api.Application.Features.Tags.Common;

public record TagResponse(Guid Id, Guid KanbanId, string Name);

public static class TagResponseMapper
{
    public static TagResponse ToResponse(this Tag tag) => new(tag.Id, tag.KanbanId, tag.Name);

    public static List<TagResponse> ToResponseList(this IEnumerable<Tag> tags) =>
        tags.Select(tag => tag.ToResponse()).ToList();
}
