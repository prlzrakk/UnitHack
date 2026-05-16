using FluentValidation;

namespace Api.Application.Features.Tags.RenameTag;

public class RenameTagValidator : AbstractValidator<RenameTagCommand>
{
    public RenameTagValidator()
    {
        RuleFor(x => x.TagId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Tag name is required")
            .MaximumLength(100)
            .WithMessage("Tag name cannot exceed 100 characters");
    }
}
