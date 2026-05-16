using FluentValidation;

namespace Api.Application.Features.Projects.GetTeamProjects;

public class GetTeamProjectsValidator : AbstractValidator<GetTeamProjectsQuery>
{
    public GetTeamProjectsValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
