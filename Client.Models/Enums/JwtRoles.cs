namespace Shared.Models.Enums;

public enum UserRole
{
    Member,
    TgBot
}

public static class UserRoleNames
{
    public const string Member = "member";
    public const string TgBot = "tg_bot";
}

public static class UserRoleExtensions
{
    public static string ToClaimValue(this UserRole role) => role switch
    {
        UserRole.Member => UserRoleNames.Member,
        UserRole.TgBot => UserRoleNames.TgBot,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };
}