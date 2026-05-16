namespace Client.Models.Enums;

public enum TokenType
{
    Access,
    Refresh
}

public static class TokenTypeNames
{
    public const string Access = "Access";
    public const string Refresh = "Refresh";
}

public static class TokenTypeExtensions
{
    public static string ToClaimValue(this TokenType role) => role switch
    {
        TokenType.Access => TokenTypeNames.Access,
        TokenType.Refresh => TokenTypeNames.Refresh,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };
}

public static class TokenClaimTypes
{
    public const string TokenType = "token_type";
}