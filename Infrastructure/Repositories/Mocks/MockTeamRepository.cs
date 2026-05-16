using Infrastructure.Entities;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Mocks;

public class MockTeamRepository(MockDataStore store) : ITeamRepository
{
    public Task<List<Team>> GetTeamsByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var teamIds = store.TeamMembers
            .Where(x => x.UserId == userId)
            .Select(x => x.TeamId)
            .ToHashSet();

        var teams = store.Teams
            .Where(x => teamIds.Contains(x.Id))
            .ToList();

        return Task.FromResult(teams);
    }

    public Task<Team?> GetTeam(Guid teamId, CancellationToken cancellationToken)
    {
        var team = store.Teams.FirstOrDefault(x => x.Id == teamId);
        return Task.FromResult(team);
    }

    public Task<Team> CreateTeam(Guid ownerId, string name, CancellationToken cancellationToken)
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = name.Trim()
        };

        var owner = new TeamMember
        {
            TeamId = team.Id,
            Team = team,
            UserId = ownerId,
            Role = TeamRole.Admin
        };

        team.Members.Add(owner);
        store.Teams.Add(team);
        store.TeamMembers.Add(owner);

        return Task.FromResult(team);
    }
}
