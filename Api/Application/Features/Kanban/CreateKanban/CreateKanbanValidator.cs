using FluentValidation;

namespace Api.Application.Features.Kanban.CreateKanban;

public class CreateKanbanValidator : AbstractValidator<CreateKanbanCommand>
{
    public CreateKanbanValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
