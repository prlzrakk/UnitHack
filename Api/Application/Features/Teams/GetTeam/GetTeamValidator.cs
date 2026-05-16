using FluentValidation;

namespace Api.Application.Features.Teams.GetTeam;

public class GetTeamValidator : AbstractValidator<GetTeamQuery>
{
    public GetTeamValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
