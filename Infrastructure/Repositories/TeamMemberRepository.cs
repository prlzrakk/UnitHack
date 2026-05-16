using Infrastructure.Db;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TeamMemberRepository(DatabaseContext context) : ITeamMemberRepository
{
    public async Task<bool> IsAdminAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        return await context.TeamMembers
            .Where(x => x.TeamId == teamId && x.UserId == userId)
            .Select(x => x.Role == TeamRole.Admin)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> IsMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}