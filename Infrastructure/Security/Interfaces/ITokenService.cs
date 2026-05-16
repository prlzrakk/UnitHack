using Shared.Models.Entities;

namespace Infrastructure.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(User user);
}
