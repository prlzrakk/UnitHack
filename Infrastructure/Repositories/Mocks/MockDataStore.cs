using Infrastructure.Constants;
using Infrastructure.Entities;
using Infrastructure.Enums;

namespace Infrastructure.Repositories.Mocks;

public class MockDataStore
{
    public static readonly Guid SeedUserId = DevSeedDefaults.UserId;

    public List<Team> Teams { get; } = [];
    public List<TeamMember> TeamMembers { get; } = [];
    public List<Project> Projects { get; } = [];
    public List<Kanban> Kanbans { get; } = [];
    public List<KanbanColumn> KanbanColumns { get; } = [];
    public List<KanbanTask> Tasks { get; } = [];
    public List<Tag> Tags { get; } = [];
    public List<TaskTag> TaskTags { get; } = [];
    public List<Notification> Notifications { get; } = [];

    public MockDataStore()
    {
        var teamId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var kanbanId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var team = new Team
        {
            Id = teamId,
            Name = "Test Team"
        };

        var user = new User
        {
            Id = SeedUserId,
            Name = DevSeedDefaults.UserName,
            Email = DevSeedDefaults.UserEmail,
            HashPassword = string.Empty
        };

        var user2 = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
            Name = "Aboba",
            Email = "test@example1.com",
            HashPassword = string.Empty
        };

        var project = new Project
        {
            Id = projectId,
            TeamId = teamId,
            Team = team,
            Name = "Test Project"
        };

        var teamMember = new TeamMember
        {
            TeamId = teamId,
            Team = team,
            UserId = SeedUserId,
            User = user,
            Role = TeamRole.Admin
        };
        
        var teamMember2 = new TeamMember
        {
            TeamId = teamId,
            Team = team,
            UserId = user2.Id,
            User = user2,
            Role = TeamRole.Admin
        };

        var kanban = new Kanban
        {
            Id = kanbanId,
            ProjectId = projectId,
            Project = project,
            Name = "Test Kanban",
            Columns = []
        };

        var columnIds = new[]
        {
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Guid.Parse("77777777-7777-7777-7777-777777777777")
        };

        var columns = KanbanDefaults.BasicColumns
            .Select((column, index) => new KanbanColumn
            {
                Id = columnIds[index],
                KanbanId = kanbanId,
                Kanban = kanban,
                Name = column.Name,
                Order = column.Order,
                Tasks = []
            })
            .ToArray();

        Teams.Add(team);
        Projects.Add(project);
        TeamMembers.Add(teamMember);
        TeamMembers.Add(teamMember2);
        Kanbans.Add(kanban);
        KanbanColumns.AddRange(columns);

        team.Members.Add(teamMember);
        team.Members.Add(teamMember2);
        team.Projects.Add(project);
        project.Kanbans.Add(kanban);

        foreach (var column in columns)
            kanban.Columns.Add(column);
    }
}
