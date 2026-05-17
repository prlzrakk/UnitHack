using FluentValidation;

namespace Api.Application.Features.TaskTags.DetachTaskTag;

public class DetachTaskTagValidator : AbstractValidator<DetachTaskTagCommand>
{
    public DetachTaskTagValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.TagId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
