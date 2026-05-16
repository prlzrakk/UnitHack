using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Projects.DeleteProject;

public class DeleteProjectHandler(
    IProjectRepository projects,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProjectCommand>
{
    public async Task Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        if (command.ProjectId == Guid.Empty)
            throw new BadRequestException("Project id is required");
        if (command.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");

        var project = await projects.GetProjectById(command.ProjectId, cancellationToken)
                      ?? throw new NotFoundException("Project not found");

        var isAdmin = await members.IsAdminAsync(project.TeamId, command.CurrentUserId, cancellationToken);
        if (!isAdmin)
            throw new ForbiddenException("Only team admin can delete projects");

        await projects.DeleteAsync(project.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
