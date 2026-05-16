using FluentValidation;

namespace Api.Application.Features.Notifications.GetNotifications;

public class GetNotificationsValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
