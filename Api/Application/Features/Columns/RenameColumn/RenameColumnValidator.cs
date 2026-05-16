using FluentValidation;

namespace Api.Application.Features.Columns.RenameColumn;

public class RenameColumnValidator : AbstractValidator<RenameColumnCommand>
{
    public RenameColumnValidator()
    {
        RuleFor(x => x.ColumnId)
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
