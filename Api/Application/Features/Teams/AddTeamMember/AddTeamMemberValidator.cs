using FluentValidation;

namespace Api.Application.Features.Teams.AddTeamMember;

public class AddTeamMemberValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
