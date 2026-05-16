using FluentValidation;

namespace Api.Application.Features.Projects.CreateProject;

public class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Project name is required")
            .MaximumLength(100)
            .WithMessage("Project name cannot exceed 100 characters");
    }
}
