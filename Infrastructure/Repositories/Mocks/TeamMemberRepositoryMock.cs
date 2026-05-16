using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class TeamMemberRepositoryMock : ITeamMemberRepository
{
    public Task<bool> IsAdminAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}