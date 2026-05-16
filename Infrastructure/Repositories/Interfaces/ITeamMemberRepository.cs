namespace Infrastructure.Repositories.Interfaces;

public interface ITeamMemberRepository
{
    Task<bool> IsAdminAsync(Guid kanbanId, Guid userId, CancellationToken cancellationToken);
}