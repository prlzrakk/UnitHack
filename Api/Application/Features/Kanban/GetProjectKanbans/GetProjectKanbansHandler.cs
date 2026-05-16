using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.GetProjectKanbans;

public class GetProjectKanbansHandler(IKanbanRepository kanbanRepository)
    : IRequestHandler<GetProjectKanbansQuery, GetProjectKanbansResponse>
{
    public async Task<GetProjectKanbansResponse> Handle(GetProjectKanbansQuery request,
        CancellationToken cancellationToken)
    {
        var kanbans = await kanbanRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        return new GetProjectKanbansResponse
        {
            Kanbans = kanbans
                .Select(kanban => new KanbanListItemResponse
                {
                    Id = kanban.Id,
                    ProjectId = kanban.ProjectId,
                    Name = kanban.Name
                })
                .ToList()
        };
    }
}