using Infrastructure.Db;
using Infrastructure.Entities;
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

    public async Task<bool> IsMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        return await context.TeamMembers
            .AnyAsync(x => x.TeamId == teamId && x.UserId == userId, cancellationToken);
    }

    public async Task<TeamMember?> GetMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        return await context.TeamMembers
            .FirstOrDefaultAsync(x => x.TeamId == teamId && x.UserId == userId, cancellationToken);
    }

    public async Task<List<TeamMember>> GetMembersByTeamIdAsync(Guid teamId, CancellationToken cancellationToken)
    {
        return await context.TeamMembers
            .Include(x => x.User)
            .Where(x => x.TeamId == teamId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAdminsAsync(Guid teamId, CancellationToken cancellationToken)
    {
        return await context.TeamMembers
            .CountAsync(x => x.TeamId == teamId && x.Role == TeamRole.Admin, cancellationToken);
    }

    public async Task<TeamMember> AddMemberAsync(
        Guid teamId,
        Guid userId,
        TeamRole role,
        CancellationToken cancellationToken)
    {
        var team = await context.Teams
            .FirstAsync(x => x.Id == teamId, cancellationToken);
        var user = await context.Users
            .FirstAsync(x => x.Id == userId, cancellationToken);

        var member = new TeamMember
        {
            TeamId = teamId,
            Team = team,
            UserId = userId,
            User = user,
            Role = role
        };

        await context.TeamMembers.AddAsync(member, cancellationToken);
        return member;
    }

    public async Task<TeamMember?> ChangeRoleAsync(
        Guid teamId,
        Guid userId,
        TeamRole role,
        CancellationToken cancellationToken)
    {
        var member = await GetMemberAsync(teamId, userId, cancellationToken);
        if (member is null)
            return null;

        member.Role = role;
        return member;
    }

    public async Task<bool> RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        var member = await GetMemberAsync(teamId, userId, cancellationToken);
        if (member is null)
            return false;

        context.TeamMembers.Remove(member);
        return true;
    }
}
