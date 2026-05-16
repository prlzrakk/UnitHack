
using Infrastructure.Entities;

namespace Infrastructure.Security.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(User user);
}