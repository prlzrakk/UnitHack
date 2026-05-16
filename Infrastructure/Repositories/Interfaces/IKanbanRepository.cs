namespace Infrastructure.Repositories.Interfaces;

public interface IKanbanRepository
{
    Task<Project?> GetProjectById(Guid projectId, CancellationToken cancellationToken);
}