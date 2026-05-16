using FluentValidation;

namespace Api.Application.Features.Notifications.CreateNotification;

public class CreateNotificationValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.KanbanId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Notification name is required")
            .MaximumLength(100)
            .WithMessage("Notification name cannot exceed 100 characters");

        RuleFor(x => x.Message)
            .MaximumLength(200)
            .WithMessage("Notification message cannot exceed 200 characters");
    }
}
