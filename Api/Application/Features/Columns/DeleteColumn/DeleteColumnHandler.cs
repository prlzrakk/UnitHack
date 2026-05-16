using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Columns.DeleteColumn;

public class DeleteColumnHandler(
    IKanbanRepository kanbans,
    IKanbanColumnRepository columns,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteColumnCommand>
{
    public async Task Handle(DeleteColumnCommand command, CancellationToken cancellationToken)
    {
        var column = await columns.GetByIdAsync(command.ColumnId, cancellationToken)
                     ?? throw new NotFoundException("Column not found");
        var kanban = await kanbans.GetByIdWithProjectAsync(column.KanbanId, cancellationToken)
                     ?? throw new NotFoundException("Kanban not found");

        var isAdmin = await members.IsAdminAsync(kanban.Project.TeamId, command.CurrentUserId, cancellationToken);
        if (!isAdmin)
            throw new ForbiddenException("Only team admin can delete columns");

        await columns.DeleteAsync(column.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
