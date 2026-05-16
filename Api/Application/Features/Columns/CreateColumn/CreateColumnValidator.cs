using FluentValidation;

namespace Api.Application.Features.Columns.CreateColumn;

public class CreateColumnValidator : AbstractValidator<CreateColumnCommand>
{
    public CreateColumnValidator()
    {
        RuleFor(x => x.KanbanId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Column name is required")
            .MaximumLength(100)
            .WithMessage("Column name cannot exceed 100 characters");
    }
}
