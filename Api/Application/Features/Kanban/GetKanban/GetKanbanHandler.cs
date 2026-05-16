using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.GetKanban;

public class GetKanbanHandler(IKanbanRepository kanbanRepository, ITeamMemberRepository teamMemberRepository)
    : IRequestHandler<GetKanbanQuery, GetKanbanResponse>
{
    public async Task<GetKanbanResponse> Handle(GetKanbanQuery request, CancellationToken cancellationToken)
    {
        if (request.KanbanId == Guid.Empty)
            throw new BadRequestException("Kanban id is required");
        if (request.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");

        var kanban = await kanbanRepository.GetByIdWithProjectAndColumnsAsync(request.KanbanId, cancellationToken);

        if (kanban is null)
            throw new NotFoundException("Kanban not found");

        var isMember =
            await teamMemberRepository.IsMemberAsync(kanban.Project.TeamId, request.CurrentUserId, cancellationToken);

        if (!isMember)
            throw new ForbiddenException("Only team member can view kanban");

        return new GetKanbanResponse
        {
            Id = kanban.Id,
            ProjectId = kanban.ProjectId,
            Name = kanban.Name,
            Columns = kanban.Columns
                .OrderBy(column => column.Order)
                .Select(column => new GetKanbanColumnResponse
                {
                    Id = column.Id,
                    Name = column.Name,
                    Order = column.Order
                })
                .ToList()
        };
    }
}
