using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockProjectRepository(MockDataStore store) : IProjectRepository
{
    public Task<Project?> GetProjectById(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var project = store.Projects
            .FirstOrDefault(x => x.Id == projectId);

        return Task.FromResult(project);
    }
}