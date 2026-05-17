using FluentValidation;

namespace Api.Application.Features.Tags.GetKanbanTags;

public class GetKanbanTagsValidator : AbstractValidator<GetKanbanTagsQuery>
{
    public GetKanbanTagsValidator()
    {
        RuleFor(x => x.KanbanId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
