using MediatR;

namespace Api.Application.Features.Kanban.GetKanban;

public record GetKanbanQuery(Guid KanbanId, Guid CurrentUserId) : IRequest<GetKanbanResponse>;