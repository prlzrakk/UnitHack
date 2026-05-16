using FluentValidation;

namespace Api.Application.Features.Kanban.DeleteKanban;

public class DeleteKanbanValidator : AbstractValidator<DeleteKanbanCommand>
{
    public DeleteKanbanValidator()
    {
        RuleFor(x => x.KanbanId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
