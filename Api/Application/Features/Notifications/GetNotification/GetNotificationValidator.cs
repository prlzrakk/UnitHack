using FluentValidation;

namespace Api.Application.Features.Notifications.GetNotification;

public class GetNotificationValidator : AbstractValidator<GetNotificationQuery>
{
    public GetNotificationValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
