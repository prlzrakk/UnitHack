using Client.Models.DTO.Response;
using Infrastructure.Security.Interfaces;
using MediatR;

namespace Api.Application.Features.Auth.Refresh;

public class RefreshHandler(ITokenService tokenService)
    : IRequestHandler<RefreshCommand, RefreshResponse>
{
    public Task<RefreshResponse> Handle(RefreshCommand command,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new RefreshResponse(
            tokenService.GenerateAccessToken(command.Email),
            tokenService.GenerateRefreshToken(command.Email)
        ));
    }
}
