using FluentValidation;

namespace Api.Application.Features.Tasks.MoveTask;

public class MoveTaskValidator : AbstractValidator<MoveTaskCommand>
{
    public MoveTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();

        RuleFor(x => x.ToColumnId)
            .NotEmpty()
            .WithMessage("Column id is required");
    }
}
