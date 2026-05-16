using FluentValidation;

namespace Api.Application.Features.Notifications.ReadAllNotifications;

public class ReadAllNotificationsValidator : AbstractValidator<ReadAllNotificationsCommand>
{
    public ReadAllNotificationsValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
