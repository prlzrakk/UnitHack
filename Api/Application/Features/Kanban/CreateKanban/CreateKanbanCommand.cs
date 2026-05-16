using MediatR;

namespace Api.Application.Features.Kanban.CreateKanban;

public record CreateKanbanCommand(Guid ProjectId, Guid CurrentUserId, string Name) : IRequest<CreateKanbanResponse>;
