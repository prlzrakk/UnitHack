using Infrastructure.Entities;
using Infrastructure.Enums;

namespace Infrastructure.Repositories.Mocks;

public class MockDataStore
{
    public static readonly Guid SeedUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public List<Team> Teams { get; } = [];
    public List<TeamMember> TeamMembers { get; } = [];
    public List<Project> Projects { get; } = [];
    public List<Kanban> Kanbans { get; } = [];
    public List<KanbanColumn> KanbanColumns { get; } = [];
    public List<KanbanTask> Tasks { get; } = [];

    public MockDataStore()
    {
        var teamId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var team = new Team
        {
            Id = teamId,
            Name = "Test Team"
        };

        Teams.Add(team);

        Projects.Add(new Project
        {
            Id = projectId,
            TeamId = teamId,
            Team = team,
            Name = "Test Project"
        });

        var teamMember = new TeamMember
        {
            TeamId = teamId,
            Team = team,
            UserId = SeedUserId,
            Role = TeamRole.Admin
        };

        TeamMembers.Add(teamMember);
        team.Members.Add(teamMember);
        team.Projects.Add(Projects[0]);

        var kanbanId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var kanban = new Kanban
        {
            Id = kanbanId,
            ProjectId = projectId,
            Project = Projects[0],
            Name = "Test Kanban",
            Columns = []
        };

        Kanbans.Add(kanban);
        Projects[0].Kanbans.Add(kanban);

        var columns = new[]
        {
            new KanbanColumn
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                KanbanId = kanbanId,
                Kanban = kanban,
                Name = "To Do",
                Order = 1000,
                Tasks = []
            },
            new KanbanColumn
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                KanbanId = kanbanId,
                Kanban = kanban,
                Name = "Done",
                Order = 2000,
                Tasks = []
            }
        };

        KanbanColumns.AddRange(columns);
        foreach (var column in columns)
            kanban.Columns.Add(column);
    }
}
