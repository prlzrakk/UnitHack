using Api.Application.Features.Projects.Common;
using MediatR;

namespace Api.Application.Features.Projects.GetProject;

public record GetProjectQuery(Guid ProjectId, Guid CurrentUserId) : IRequest<ProjectResponse>;
