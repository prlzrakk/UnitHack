using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProjectRepository(DatabaseContext context) : IProjectRepository
{
    public async Task<Project> AddAsync(Guid teamId, string name, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            Name = name.Trim()
        };

        await context.Projects.AddAsync(project, cancellationToken);
        return project;
    }

    public async Task<Project?> GetProjectById(Guid projectId, CancellationToken cancellationToken)
    {
        return await context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);
    }

    public async Task<List<Project>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken)
    {
        return await context.Projects
            .Where(x => x.TeamId == teamId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);

        if (project is null)
            return false;

        context.Projects.Remove(project);
        return true;
    }
}
