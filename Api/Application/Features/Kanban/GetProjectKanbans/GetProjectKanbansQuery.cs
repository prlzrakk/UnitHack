using MediatR;

namespace Api.Application.Features.Kanban.GetProjectKanbans;

public record GetProjectKanbansQuery(Guid ProjectId, Guid UserId) : IRequest<GetProjectKanbansResponse>;