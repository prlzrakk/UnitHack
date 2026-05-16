namespace Infrastructure.Security;

public class JwtSettings
{
    public TimeSpan ExpiresAccess { get; init; }
    public TimeSpan ExpiresRefresh { get; init; }
    public bool AllowNonExpiringTokens { get; init; }

    public string SecretKey { get; init; } = string.Empty;
}