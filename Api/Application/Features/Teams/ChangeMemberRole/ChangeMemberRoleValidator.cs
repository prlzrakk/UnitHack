using FluentValidation;

namespace Api.Application.Features.Teams.ChangeMemberRole;

public class ChangeMemberRoleValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Role)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Role is required")
            .Must(role => role.HasValue && Enum.IsDefined(role.Value))
            .WithMessage("Role is invalid");
    }
}
