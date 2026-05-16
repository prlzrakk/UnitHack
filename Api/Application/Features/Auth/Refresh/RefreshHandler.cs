using Infrastructure.Interfaces;
using MediatR;
using Shared.Models.DTO.Response;

namespace Api.Application.Features.Auth.Refresh;

public class RefreshHandler(ITokenService tokenService)
    : IRequestHandler<RefreshCommand, RefreshResponse>
{
    public Task<RefreshResponse> Handle(RefreshCommand command,
        CancellationToken cancellationToken)
    {
        var username = command.Username;
        return Task.FromResult(new RefreshResponse(
            tokenService.GenerateAccessToken(username),
            tokenService.GenerateRefreshToken(username)
        ));
    }
}