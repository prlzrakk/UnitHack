using Api.Application.Common.Exceptions;
using Api.Application.Features.Tags.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.GetKanban;

public class GetKanbanHandler(IKanbanRepository kanbanRepository, ITeamMemberRepository teamMemberRepository)
    : IRequestHandler<GetKanbanQuery, GetKanbanResponse>
{
    public async Task<GetKanbanResponse> Handle(GetKanbanQuery request, CancellationToken cancellationToken)
    {
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
                    Order = column.Order,
                    Tasks = kanban.Tasks
                        .Where(task => task.ColumnId == column.Id)
                        .OrderBy(task => task.Order)
                        .Select(task => new GetKanbanTaskResponse
                        {
                            Id = task.Id,
                            KanbanId = task.KanbanId,
                            ColumnId = task.ColumnId,
                            UserId = task.UserId,
                            Name = task.Name,
                            Description = task.Description,
                            Priority = task.Priority,
                            CreatedAt = task.CreatedAt,
                            Deadline = task.Deadline,
                            Order = task.Order,
                            Tags = task.TaskTags
                                .Select(taskTag => taskTag.Tag.ToResponse())
                                .OrderBy(tag => tag.Name)
                                .ToList()
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
