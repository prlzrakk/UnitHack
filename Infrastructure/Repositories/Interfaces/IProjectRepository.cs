using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetProjectById(Guid projectId, CancellationToken cancellationToken);
}