using FluentValidation;

namespace Api.Application.Features.Notifications.GetUnreadNotifications;

public class GetUnreadNotificationsValidator : AbstractValidator<GetUnreadNotificationsQuery>
{
    public GetUnreadNotificationsValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
