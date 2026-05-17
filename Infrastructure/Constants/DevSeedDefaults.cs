namespace Infrastructure.Constants;

public static class DevSeedDefaults
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TeamId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ProjectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid KanbanId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public const string UserEmail = "test@example.com";
    public const string UserName = "Test User";
    public const string UserPassword = "password";

    public const string TeamName = "Test Team";
    public const string ProjectName = "Test Project";
    public const string KanbanName = "Test Kanban";
}
