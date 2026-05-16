using FluentValidation;

namespace WebApplication1.Application.Features.Kanban.ChangeKanban;

public class ChangeKanbanValidator : AbstractValidator<ChangeKanbanCommand>
{
    public ChangeKanbanValidator()
    {
        RuleFor(x => x.KanbanId).NotEmpty();
        
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");
    }
}