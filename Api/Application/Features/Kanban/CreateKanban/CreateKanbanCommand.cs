using MediatR;

namespace WebApplication1.Application.Features.Kanban.CreateKanban;

public record CreateKanbanCommand(Guid ProjectId, string Name) : IRequest<CreateKanbanResponse>;