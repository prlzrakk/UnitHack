using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.CreateKanban;

public class CreateKanbanHandler(
    IProjectRepository projectRepository,
    ITeamMemberRepository teamMemberRepository,
    IKanbanRepository kanbanRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateKanbanCommand, CreateKanbanResponse>
{
    public async Task<CreateKanbanResponse> Handle(CreateKanbanCommand request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetProjectById(request.ProjectId, cancellationToken);

        if (project is null)
            throw new NotFoundException("Project not found");

        var isAdmin = await teamMemberRepository.IsAdminAsync(project.TeamId, request.CurrentUserId, cancellationToken);
        if (!isAdmin)
            throw new ForbiddenException("Only team admin can create kanban");

        var kanban = await kanbanRepository.AddAsync(project.Id, request.Name, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateKanbanResponse
        {
            Id = kanban.Id,
            ProjectId = kanban.ProjectId,
            Name = kanban.Name,
            Columns = kanban.Columns
                .OrderBy(x => x.Order)
                .Select(x => new KanbanColumnResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Order = x.Order
                })
                .ToList()
        };
    }
}
