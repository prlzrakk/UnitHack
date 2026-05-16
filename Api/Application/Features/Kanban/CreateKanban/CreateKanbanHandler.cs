using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.CreateKanban;

public class CreateKanbanHandler(
    IProjectRepository projectRepository,
    IKanbanRepository kanbanRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateKanbanCommand, CreateKanbanResponse>
{
    public async Task<CreateKanbanResponse> Handle(CreateKanbanCommand request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetProjectById(request.ProjectId, cancellationToken);

        if (project is null)
            throw new Exception("Project not found");

        var kanban = new Infrastructure.Entities.Kanban
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
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

        kanbanRepository.Add(kanban);

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