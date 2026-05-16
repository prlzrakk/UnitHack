using MediatR;

namespace Api.Application.Features.Kanban.CreateKanban;

public record CreateKanbanCommand(Guid ProjectId, string Name) : IRequest<CreateKanbanResponse>;