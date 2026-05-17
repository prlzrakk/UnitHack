using FluentValidation;

namespace Api.Application.Features.TaskTags.AttachTaskTag;

public class AttachTaskTagValidator : AbstractValidator<AttachTaskTagCommand>
{
    public AttachTaskTagValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.TagId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
