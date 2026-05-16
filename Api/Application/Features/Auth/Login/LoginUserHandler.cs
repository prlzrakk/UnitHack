using Client.Models.DTO.Response;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security.Interfaces;
using MediatR;
using WebApplication1.Application.Common.Exceptions;

namespace WebApplication1.Application.Features.Auth.Login;

public class LoginUserHandler(IUserRepository users, ITokenService tokenService)
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<LoginUserResponse> Handle(LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var username = command.Email;

        var loginStatus = await users.LoginUser(username, command.Password);

        if (loginStatus)
            return new LoginUserResponse(tokenService.GenerateAccessToken(username),
                tokenService.GenerateRefreshToken(username));
        throw new UnauthorizedException("Incorrect password or username");
    }
}