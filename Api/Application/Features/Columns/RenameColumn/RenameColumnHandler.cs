using Api.Application.Common.Exceptions;
using Api.Application.Features.Columns.Common;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Columns.RenameColumn;

public class RenameColumnHandler(
    IKanbanRepository kanbans,
    IKanbanColumnRepository columns,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<RenameColumnCommand, ColumnResponse>
{
    public async Task<ColumnResponse> Handle(RenameColumnCommand command, CancellationToken cancellationToken)
    {
        var column = await columns.GetByIdAsync(command.ColumnId, cancellationToken)
                     ?? throw new NotFoundException("Column not found");
        var kanban = await kanbans.GetByIdWithProjectAsync(column.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        var isAdmin = await members.IsAdminAsync(kanban.Project.TeamId, command.CurrentUserId, cancellationToken);
        if (!isAdmin)
            throw new ForbiddenException("Only team admin can rename columns");

        column.Name = command.Name.Trim();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ColumnResponse(column.Id, column.KanbanId, column.Name, column.Order);
    }
}
