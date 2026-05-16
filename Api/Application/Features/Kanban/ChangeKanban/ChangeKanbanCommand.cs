using MediatR;

namespace Api.Application.Features.Kanban.ChangeKanban;

public record ChangeKanbanCommand(Guid KanbanId, string Name, Guid CurrentUserId) : IRequest<ChangeKanbanResponse>;