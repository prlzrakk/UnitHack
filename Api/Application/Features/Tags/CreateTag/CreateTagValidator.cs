using FluentValidation;

namespace Api.Application.Features.Tags.CreateTag;

public class CreateTagValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagValidator()
    {
        RuleFor(x => x.KanbanId)
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
