using FluentValidation;

namespace Api.Application.Features.Notifications.ReadNotification;

public class ReadNotificationValidator : AbstractValidator<ReadNotificationCommand>
{
    public ReadNotificationValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
