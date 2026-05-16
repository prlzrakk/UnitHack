using FluentValidation;

namespace WebApplication1.Application.Features.Kanban.CreateKanban;

public class CreateKanbanValidator : AbstractValidator<CreateKanbanCommand>
{
    public CreateKanbanValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}