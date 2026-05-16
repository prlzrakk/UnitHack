using MediatR;

namespace Api.Application.Features.Projects.DeleteProject;

public record DeleteProjectCommand(Guid ProjectId, Guid CurrentUserId) : IRequest;
