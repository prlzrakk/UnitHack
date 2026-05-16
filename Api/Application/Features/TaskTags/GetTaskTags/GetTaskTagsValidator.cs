using FluentValidation;

namespace Api.Application.Features.TaskTags.GetTaskTags;

public class GetTaskTagsValidator : AbstractValidator<GetTaskTagsQuery>
{
    public GetTaskTagsValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
