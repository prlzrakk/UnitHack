using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<Project> AddAsync(Project project, CancellationToken cancellationToken);
    Task<Project?> GetProjectById(Guid projectId, CancellationToken cancellationToken);
    Task<List<Project>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid projectId, CancellationToken cancellationToken);
}
