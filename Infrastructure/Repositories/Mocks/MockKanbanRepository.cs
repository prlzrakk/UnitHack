using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockKanbanRepository(MockDataStore store) : IKanbanRepository
{
    public void Add(Kanban kanban)
    {
        store.Kanbans.Add(kanban);

        if (kanban.Columns is not null)
        {
            store.KanbanColumns.AddRange(kanban.Columns);
        }
    }

    public Task<bool> DeleteAsync(
        Guid kanbanId,
        CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans
            .FirstOrDefault(x => x.Id == kanbanId);

        if (kanban is null)
        {
            return Task.FromResult(false);
        }

        store.Kanbans.Remove(kanban);

        store.KanbanColumns.RemoveAll(x => x.KanbanId == kanbanId);

        return Task.FromResult(true);
    }

    public Task<Kanban?> GetByIdWithProjectAsync(
        Guid kanbanId,
        CancellationToken cancellationToken)
    {
        var kanban = store.Kanbans
            .FirstOrDefault(x => x.Id == kanbanId);

        if (kanban is not null && kanban.Project is null)
        {
            kanban.Project = store.Projects
                .FirstOrDefault(x => x.Id == kanban.ProjectId);
        }

        return Task.FromResult(kanban);
    }

    public Task<List<Kanban>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var kanbans = store.Kanbans
            .Where(x => x.ProjectId == projectId)
            .ToList();

        return Task.FromResult(kanbans);
    }
}