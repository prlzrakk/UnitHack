using FluentValidation;

namespace Api.Application.Features.Kanban.GetProjectKanbans;

public class GetProjectKanbansValidator : AbstractValidator<GetProjectKanbansQuery>
{
    public GetProjectKanbansValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
