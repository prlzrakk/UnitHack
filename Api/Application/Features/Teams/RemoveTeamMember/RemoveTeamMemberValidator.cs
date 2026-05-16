using FluentValidation;

namespace Api.Application.Features.Teams.RemoveTeamMember;

public class RemoveTeamMemberValidator : AbstractValidator<RemoveTeamMemberCommand>
{
    public RemoveTeamMemberValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
