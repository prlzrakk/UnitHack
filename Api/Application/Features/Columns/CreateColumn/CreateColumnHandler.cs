using Api.Application.Common.Exceptions;
using Api.Application.Features.Columns.Common;
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
        var kanban = await kanbans.GetByIdWithProjectAsync(command.KanbanId, cancellationToken)
                      ?? throw new NotFoundException("Kanban not found");

        var isAdmin = await members.IsAdminAsync(kanban.Project.TeamId, command.CurrentUserId, cancellationToken);
        if (!isAdmin)
            throw new ForbiddenException("Only team admin can create columns");

        var column = await columns.AddAsync(kanban.Id, command.Name, command.Order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ColumnResponse(column.Id, column.KanbanId, column.Name, column.Order);
    }
}
