using FluentValidation;

namespace Api.Application.Features.Teams.CreateTeam;

public class CreateTeamValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Team name is required")
            .MaximumLength(100)
            .WithMessage("Team name cannot exceed 100 characters");
    }
}
