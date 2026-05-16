using Api.Application.Common.Exceptions;
using Api.Application.Features.Columns.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Columns.CreateColumn;

public class CreateColumnHandler(
    IKanbanRepository kanbans,
    IKanbanColumnRepository columns,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateColumnCommand, ColumnResponse>
{
    public async Task<ColumnResponse> Handle(CreateColumnCommand command, CancellationToken cancellationToken)
    {
        if (command.KanbanId == Guid.Empty)
            throw new BadRequestException("Kanban id is required");
        if (command.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new BadRequestException("Column name is required");

        var kanban = await kanbans.GetByIdWithProjectAsync(command.KanbanId, cancellationToken)
                      ?? throw new NotFoundException("Kanban not found");

        var isAdmin = await members.IsAdminAsync(kanban.Project.TeamId, command.CurrentUserId, cancellationToken);
        if (!isAdmin)
            throw new ForbiddenException("Only team admin can create columns");

        var order = command.Order ?? (kanban.Columns.Count == 0 ? 1000 : kanban.Columns.Max(x => x.Order) + 1000);
        var column = await columns.AddAsync(new KanbanColumn
        {
            Id = Guid.NewGuid(),
            KanbanId = kanban.Id,
            Kanban = kanban,
            Name = command.Name.Trim(),
            Order = order,
            Tasks = []
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ColumnResponse(column.Id, column.KanbanId, column.Name, column.Order);
    }
}
