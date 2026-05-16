using Api.Application.Common.Exceptions;
using MediatR;
using Client.Models.DTO.Response;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security.Interfaces;

namespace Api.Application.Features.Auth.Refresh;

public class RefreshHandler(ITokenService tokenService, IUserRepository users)
    : IRequestHandler<RefreshCommand, RefreshResponse>
{
    public async Task<RefreshResponse> Handle(RefreshCommand command,
        CancellationToken cancellationToken)
    {
        var user = await users.GetUser(command.UserId)
                   ?? throw new UnauthorizedException("User not found");

        return new RefreshResponse(
            tokenService.GenerateAccessToken(user),
            tokenService.GenerateRefreshToken(user)
        );
    }
}