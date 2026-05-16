using FluentValidation;

namespace Api.Application.Features.Users.GetMe;

public class GetMeUserValidator : AbstractValidator<GetMeUserQuery>
{
    public GetMeUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
