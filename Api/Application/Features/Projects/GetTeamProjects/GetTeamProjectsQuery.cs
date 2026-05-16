using Api.Application.Features.Projects.Common;
using MediatR;

namespace Api.Application.Features.Projects.GetTeamProjects;

public record GetTeamProjectsQuery(Guid TeamId, Guid CurrentUserId) : IRequest<List<ProjectResponse>>;
