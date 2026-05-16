using FluentValidation;

namespace Api.Application.Features.Kanban.GetKanban;

public class GetKanbanValidator : AbstractValidator<GetKanbanQuery>
{
    public GetKanbanValidator()
    {
        RuleFor(x => x.KanbanId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
