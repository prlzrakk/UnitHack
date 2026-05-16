namespace Infrastructure.Security.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(string username);
    string GenerateRefreshToken(string username);
}