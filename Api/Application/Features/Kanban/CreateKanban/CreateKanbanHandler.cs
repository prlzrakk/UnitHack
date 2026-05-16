using Infrastructure.Entities;
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
        if (request.ProjectId == Guid.Empty)
            throw new BadRequestException("Project id is required");
        if (request.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BadRequestException("Kanban name is required");

        var project = await projectRepository.GetProjectById(request.ProjectId, cancellationToken);

        if (project is null)
            throw new NotFoundException("Project not found");

        var isAdmin = await teamMemberRepository.IsAdminAsync(project.TeamId, request.CurrentUserId, cancellationToken);
        if (!isAdmin)
            throw new ForbiddenException("Only team admin can create kanban");

        var kanban = new Infrastructure.Entities.Kanban
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Project = project,
            Name = request.Name.Trim()
        };

        var columns = new List<KanbanColumn>
        {
            new()
            {
                Id = Guid.NewGuid(),
                KanbanId = kanban.Id,
                Name = "To Do",
                Order = 1000
            },
            new()
            {
                Id = Guid.NewGuid(),
                KanbanId = kanban.Id,
                Name = "In Progress",
                Order = 2000
            },
            new()
            {
                Id = Guid.NewGuid(),
                KanbanId = kanban.Id,
                Name = "Done",
                Order = 3000
            }
        };


        kanban.Columns = columns;

        await kanbanRepository.AddAsync(kanban, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateKanbanResponse
        {
            Id = kanban.Id,
            ProjectId = kanban.ProjectId,
            Name = kanban.Name,
            Columns = columns
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
