using Api.Application.Features.Projects.Common;
using MediatR;

namespace Api.Application.Features.Projects.CreateProject;

public record CreateProjectCommand(Guid TeamId, Guid CurrentUserId, string Name) : IRequest<ProjectResponse>;
