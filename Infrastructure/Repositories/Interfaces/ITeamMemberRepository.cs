namespace Infrastructure.Repositories.Interfaces;

public interface ITeamMemberRepository
{
    Task<bool> IsAdminAsync(Guid teamId, Guid userId, CancellationToken cancellationToken);
    Task<bool> IsMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken);
}