using Infrastructure.Db;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories;

public class UnitOfWorkRepository(DatabaseContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}