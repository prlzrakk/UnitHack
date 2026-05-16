using Infrastructure.Entities;

namespace Infrastructure.Repositories.Interfaces;

public interface ITeamRepository
{
    Task<Team> CreateTeam(Guid ownerId, string name, CancellationToken cancellationToken);
    Task<Team?> GetTeam(Guid teamId, CancellationToken cancellationToken);
    Task<List<Team>> GetTeamsByUserId(Guid userId, CancellationToken cancellationToken);
}
