using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories;

public class ProjectRepository(DatabaseContext context) : IProjectRepository
{
    public async Task<Project?> GetProjectById(Guid projectId, CancellationToken cancellationToken)
    {
        return await context.Projects.FindAsync(projectId, cancellationToken);
    }
}