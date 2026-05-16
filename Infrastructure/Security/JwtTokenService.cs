using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Client.Models.Enums;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Models.Enums;

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

    private static List<Claim> CreateBaseClaims(string email, TokenType type, UserRole role, bool isNeverExpires) =>
    [
        new("email", email),
        new("role", role.ToClaimValue()),
        new(TokenClaimTypes.TokenType, type.ToClaimValue()),
        new("never_expires", isNeverExpires.ToString())
    ];


    private string GenerateToken(string username, TokenType type, UserRole role, bool isNeverExpires)
    {
        var claims = CreateBaseClaims(username, type, role, isNeverExpires);

        var jwtToken = new JwtSecurityToken(
            expires: DateTime.UtcNow.Add(type is TokenType.Access ? settings.ExpiresAccess : settings.ExpiresRefresh),
            claims: claims,
            signingCredentials: signingCredentials);
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    public string GenerateAccessToken(string username) =>
        GenerateToken(username, TokenType.Access, UserRole.Member, false);

    public string GenerateRefreshToken(string username) =>
        GenerateToken(username, TokenType.Refresh, UserRole.Member, false);
}