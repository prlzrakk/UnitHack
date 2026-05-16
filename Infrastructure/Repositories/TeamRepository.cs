using Infrastructure.Db;
using Infrastructure.Entities;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TeamRepository(DatabaseContext context) : ITeamRepository
{
    public async Task<Team> CreateTeam(Guid ownerId, string name, CancellationToken cancellationToken)
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = name.Trim()
        };

        team.Members.Add(new TeamMember
        {
            TeamId = team.Id,
            Team = team,
            UserId = ownerId,
            Role = TeamRole.Admin
        });

        await context.Teams.AddAsync(team, cancellationToken);
        return team;
    }

    public async Task<Team?> GetTeam(Guid teamId, CancellationToken cancellationToken)
    {
        return await context.Teams
            .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);
    }

    public async Task<List<Team>> GetTeamsByUserId(Guid userId, CancellationToken cancellationToken)
    {
        return await context.TeamMembers
            .Where(x => x.UserId == userId)
            .Select(x => x.Team)
            .ToListAsync(cancellationToken);
    }
}
