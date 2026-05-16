using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public static class JwtLifetimeValidator
{
    public static bool Validate(DateTime? notBefore, DateTime? expires, SecurityToken token, JwtSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var now = DateTime.UtcNow;
        IEnumerable<Claim> claims = token switch
        {
            JwtSecurityToken jwt => jwt.Claims,
            JsonWebToken jsonJwt => jsonJwt.Claims,
            _ => []
        };
        var neverExpiresValue = claims.FirstOrDefault(c => c.Type == "never_expires")?.Value;

        if (settings.AllowNonExpiringTokens
            && !string.IsNullOrEmpty(neverExpiresValue)
            && string.Equals(neverExpiresValue, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            return notBefore is null || now >= notBefore.Value;

        if (expires is null)
            return false;

        return (notBefore is null || now >= notBefore.Value) && now < expires.Value;
    }
}