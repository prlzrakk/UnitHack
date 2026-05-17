using FluentValidation;

namespace Api.Application.Features.Tasks.UpdateTask;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Task name is required")
            .MaximumLength(100)
            .WithMessage("Task name cannot exceed 100 characters");

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleForEach(x => x.TagIds)
            .NotEmpty()
            .WithMessage("Tag id is required");

        RuleFor(x => x.Priority)
            .Must(priority => Enum.IsDefined(priority))
            .WithMessage("Priority is invalid");
    }
}
