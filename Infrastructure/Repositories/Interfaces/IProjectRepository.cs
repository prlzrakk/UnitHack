using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface IProjectRepository
{
    // TODO: вместо ExampleEntitie здесь будет сущность таблицы Project
    Task<ExampleEntitie?> GetProjectById(Guid projectId, CancellationToken cancellationToken);
}