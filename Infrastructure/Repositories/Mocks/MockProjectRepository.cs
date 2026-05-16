using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockProjectRepository(MockDataStore store) : IProjectRepository
{
    public Task<Project> AddAsync(Project project, CancellationToken cancellationToken)
    {
        var team = store.Teams.First(x => x.Id == project.TeamId);
        project.Team = team;

        store.Projects.Add(project);
        team.Projects.Add(project);

        return Task.FromResult(project);
    }

    public Task<Project?> GetProjectById(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var project = store.Projects
            .FirstOrDefault(x => x.Id == projectId);

        return Task.FromResult(project);
    }

    public Task<List<Project>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var projects = store.Projects
            .Where(x => x.TeamId == teamId)
            .ToList();

        return Task.FromResult(projects);
    }

    public Task<bool> DeleteAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = store.Projects.FirstOrDefault(x => x.Id == projectId);
        if (project is null)
            return Task.FromResult(false);

        var kanbanIds = store.Kanbans
            .Where(x => x.ProjectId == projectId)
            .Select(x => x.Id)
            .ToHashSet();

        var columnIds = store.KanbanColumns
            .Where(x => kanbanIds.Contains(x.KanbanId))
            .Select(x => x.Id)
            .ToHashSet();

        store.Tasks.RemoveAll(x => kanbanIds.Contains(x.KanbanId) || columnIds.Contains(x.ColumnId));
        store.KanbanColumns.RemoveAll(x => kanbanIds.Contains(x.KanbanId));
        store.Kanbans.RemoveAll(x => x.ProjectId == projectId);
        store.Projects.Remove(project);
        project.Team?.Projects.Remove(project);

        return Task.FromResult(true);
    }
}
