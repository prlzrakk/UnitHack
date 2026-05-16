using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public class JwtSecretProvider
{
    private const int MinimumSecretLength = 32;

    public static string ResolveSecretFromEnvironment(string environmentVariableName)
    {
        if (string.IsNullOrWhiteSpace(environmentVariableName))
            throw new InvalidOperationException("JWT secret environment variable name is missing in configuration.");

        var secret = Environment.GetEnvironmentVariable(environmentVariableName);

        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException(
                $"JWT secret value was not found. Ensure the {environmentVariableName} environment variable is set.");

        if (secret.Length < MinimumSecretLength)
            throw new InvalidOperationException(
                $"JWT secret must be at least {MinimumSecretLength} characters long.");
        return secret;
    }

    public static SymmetricSecurityKey CreateSigningKey(string environmentVariableName)
    {
        var secret = ResolveSecretFromEnvironment(environmentVariableName);
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }
}