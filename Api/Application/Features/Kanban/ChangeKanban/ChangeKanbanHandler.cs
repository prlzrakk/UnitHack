using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Kanban.ChangeKanban;

public class ChangeKanbanHandler(
    IKanbanRepository kanbanRepository,
    ITeamMemberRepository teamMemberRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeKanbanCommand, ChangeKanbanResponse>
{
    public async Task<ChangeKanbanResponse> Handle(ChangeKanbanCommand request, CancellationToken cancellationToken)
    {
        if (request.KanbanId == Guid.Empty)
            throw new BadRequestException("Kanban id is required");
        if (request.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BadRequestException("Kanban name is required");

        var kanban = await kanbanRepository.GetByIdWithProjectAsync(request.KanbanId, cancellationToken);

        if (kanban is null)
            throw new NotFoundException("Kanban not found");

        var isAdmin =
            await teamMemberRepository.IsAdminAsync(kanban.Project.TeamId, request.CurrentUserId, cancellationToken);

        if (!isAdmin)
            throw new ForbiddenException("Only team admin can change kanban");

        kanban.Name = request.Name.Trim();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChangeKanbanResponse
        {
            Id = kanban.Id,
            ProjectId = kanban.ProjectId,
            Name = kanban.Name
        };
    }
}
