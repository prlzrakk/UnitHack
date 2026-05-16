using MediatR;

namespace Api.Application.Features.Kanban.DeleteKanban;

public record DeleteKanbanCommand(Guid KanbanId) : IRequest;