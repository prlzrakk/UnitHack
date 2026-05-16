using MediatR;

namespace WebApplication1.Application.Features.Kanban.DeleteKanban;

public record DeleteKanbanCommand(Guid KanbanId) : IRequest;