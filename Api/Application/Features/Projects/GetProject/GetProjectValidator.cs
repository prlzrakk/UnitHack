using FluentValidation;

namespace Api.Application.Features.Projects.GetProject;

public class GetProjectValidator : AbstractValidator<GetProjectQuery>
{
    public GetProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.CurrentUserId)
            .NotEmpty();
    }
}
