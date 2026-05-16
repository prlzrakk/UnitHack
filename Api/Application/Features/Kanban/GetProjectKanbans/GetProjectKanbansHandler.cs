using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.GetProjectKanbans;

public class GetProjectKanbansHandler(
    IProjectRepository projectRepository,
    ITeamMemberRepository teamMemberRepository,
    IKanbanRepository kanbanRepository)
    : IRequestHandler<GetProjectKanbansQuery, GetProjectKanbansResponse>
{
    public async Task<GetProjectKanbansResponse> Handle(GetProjectKanbansQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProjectId == Guid.Empty)
            throw new BadRequestException("Project id is required");
        if (request.UserId == Guid.Empty)
            throw new BadRequestException("User id is required");

        var project = await projectRepository.GetProjectById(request.ProjectId, cancellationToken)
                      ?? throw new NotFoundException("Project not found");

        var isMember = await teamMemberRepository.IsMemberAsync(project.TeamId, request.UserId, cancellationToken);
        if (!isMember)
            throw new ForbiddenException("Only team member can view kanbans");

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
