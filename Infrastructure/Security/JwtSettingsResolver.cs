using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Security;

public static class JwtSettingsResolver
{
    public static JwtSettings Resolve(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection("JwtSettings");
        if (!section.Exists())
            throw new InvalidOperationException("JwtSettings are missing in configuration.");

        return new JwtSettings
        {
            ExpiresAccess = ResolveTimeSpan(section, nameof(JwtSettings.ExpiresAccess)),
            ExpiresRefresh = ResolveTimeSpan(section, nameof(JwtSettings.ExpiresRefresh)),
            AllowNonExpiringTokens = section.GetValue<bool>(nameof(JwtSettings.AllowNonExpiringTokens)),
            SecretKey = section[nameof(JwtSettings.SecretKey)]
                        ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing in configuration.")
        };
    }

    private static TimeSpan ResolveTimeSpan(IConfigurationSection section, string key)
    {
        var rawValue = section[key];
        if (string.IsNullOrWhiteSpace(rawValue))
            throw new InvalidOperationException($"JwtSettings:{key} is missing in configuration.");

        if (TimeSpan.TryParse(rawValue, CultureInfo.InvariantCulture, out var value))
            return value;

        var environmentValue = Environment.GetEnvironmentVariable(rawValue);
        if (!string.IsNullOrWhiteSpace(environmentValue)
            && TimeSpan.TryParse(environmentValue, CultureInfo.InvariantCulture, out value))
            return value;

        throw new InvalidOperationException(
            $"JwtSettings:{key} must be a valid TimeSpan or an environment variable name containing one. " +
            $"Current value: '{rawValue}'.");
    }
}
