using Infrastructure.Entities;

namespace Infrastructure.Repositories.Mocks;

public class MockDataStore
{
    public List<Project> Projects { get; } = [];
    public List<Kanban> Kanbans { get; } = [];
    public List<KanbanColumn> KanbanColumns { get; } = [];
    public List<TeamMember> TeamMembers { get; } = [];

    public MockDataStore()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var teamId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        Projects.Add(new Project
        {
            Id = projectId,
            TeamId = teamId,
            Name = "Test Project"
        });

        TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = userId,
            Role = "Admin"
        });

        var kanbanId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        Kanbans.Add(new Kanban
        {
            Id = kanbanId,
            ProjectId = projectId,
            Project = Projects[0],
            Name = "Test Kanban",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Columns = []
        });
    }
}