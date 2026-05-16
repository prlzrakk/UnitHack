using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Client.Models.Enums;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Models.Entities;

namespace Infrastructure.Security;

public class JwtTokenService : ITokenService
{
    private readonly JwtSettings settings;
    private readonly SigningCredentials signingCredentials;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        settings = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (settings.ExpiresAccess <= TimeSpan.Zero)
            throw new InvalidOperationException("JWT access expiry must be a positive TimeSpan value.");
        if (settings.ExpiresRefresh <= TimeSpan.Zero)
            throw new InvalidOperationException("JWT refresh expiry must be a positive TimeSpan value.");

        var secret = JwtSecretProvider.ResolveSecretFromEnvironment(settings.SecretKey);
        signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256);
    }

    private static List<Claim> CreateBaseClaims(User user, TokenType type, bool isNeverExpires) =>
    [
        new("user_id", user.Id.ToString()),
        new("email", user.Email),
        new("name", user.Name),
        new(TokenClaimTypes.TokenType, type.ToClaimValue()),
        new("never_expires", isNeverExpires.ToString())
    ];

    private string GenerateToken(User user, TokenType type, bool isNeverExpires)
    {
        var claims = CreateBaseClaims(user, type, isNeverExpires);

        var jwtToken = new JwtSecurityToken(
            expires: DateTime.UtcNow.Add(type is TokenType.Access ? settings.ExpiresAccess : settings.ExpiresRefresh),
            claims: claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    public string GenerateAccessToken(User user) =>
        GenerateToken(user, TokenType.Access, false);

    public string GenerateRefreshToken(User user) =>
        GenerateToken(user, TokenType.Refresh, false);
}
