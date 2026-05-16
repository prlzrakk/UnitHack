using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class TeamMemberRepositoryMock(MockDataStore store) : ITeamMemberRepository
{
    public Task<bool> IsAdminAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        var isAdmin = store.TeamMembers.Any(x =>
            x.TeamId == teamId &&
            x.UserId == userId &&
            x.Role == TeamRole.Admin);

        return Task.FromResult(isAdmin);
    }
}