using FluentValidation;

namespace Api.Application.Features.Tasks.DeleteTask;

public class DeleteTaskValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
