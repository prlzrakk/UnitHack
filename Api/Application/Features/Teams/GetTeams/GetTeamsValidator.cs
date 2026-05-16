using FluentValidation;

namespace Api.Application.Features.Teams.GetTeams;

public class GetTeamsValidator : AbstractValidator<GetTeamsQuery>
{
    public GetTeamsValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
