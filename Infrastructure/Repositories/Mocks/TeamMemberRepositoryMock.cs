using Infrastructure.Entities;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class TeamMemberRepositoryMock(MockDataStore store) : ITeamMemberRepository
{
    public Task<TeamMember?> GetMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        var member = store.TeamMembers.FirstOrDefault(x =>
            x.TeamId == teamId
            && x.UserId == userId);

        return Task.FromResult(member);
    }

    public Task<List<TeamMember>> GetMembersByTeamIdAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var members = store.TeamMembers
            .Where(x => x.TeamId == teamId)
            .ToList();

        return Task.FromResult(members);
    }

    public Task<int> CountAdminsAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var count = store.TeamMembers.Count(x =>
            x.TeamId == teamId
            && x.Role == TeamRole.Admin);

        return Task.FromResult(count);
    }

    public Task<TeamMember> AddMemberAsync(
        Guid teamId,
        Guid userId,
        TeamRole role,
        CancellationToken cancellationToken)
    {
        var team = store.Teams.First(x => x.Id == teamId);
        var member = new TeamMember
        {
            TeamId = teamId,
            Team = team,
            UserId = userId,
            Role = role
        };

        store.TeamMembers.Add(member);
        team.Members.Add(member);

        return Task.FromResult(member);
    }

    public Task<TeamMember?> ChangeRoleAsync(
        Guid teamId,
        Guid userId,
        TeamRole role,
        CancellationToken cancellationToken)
    {
        var member = store.TeamMembers.FirstOrDefault(x =>
            x.TeamId == teamId
            && x.UserId == userId);

        if (member is null)
            return Task.FromResult<TeamMember?>(null);

        member.Role = role;
        return Task.FromResult<TeamMember?>(member);
    }

    public Task<bool> IsAdminAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        var isAdmin = store.TeamMembers.Any(x =>
            x.TeamId == teamId
            && x.UserId == userId
            && x.Role == TeamRole.Admin);

        return Task.FromResult(isAdmin);
    }

    public Task<bool> IsMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        var isMember = store.TeamMembers.Any(x =>
            x.TeamId == teamId
            && x.UserId == userId);

        return Task.FromResult(isMember);
    }

    public Task<bool> RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        var member = store.TeamMembers.FirstOrDefault(x =>
            x.TeamId == teamId
            && x.UserId == userId);

        if (member is null)
            return Task.FromResult(false);

        store.TeamMembers.Remove(member);
        var team = store.Teams.FirstOrDefault(x => x.Id == teamId);
        team?.Members.Remove(member);

        return Task.FromResult(true);
    }
}
