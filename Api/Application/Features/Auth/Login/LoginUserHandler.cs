using Api.Application.Common.Exceptions;
using MediatR;
using Client.Models.DTO.Response;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security.Interfaces.Infrastructure.Interfaces;

namespace Api.Application.Features.Auth.Login;

public class LoginUserHandler(IUserRepository users, ITokenService tokenService)
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<LoginUserResponse> Handle(LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        var loginStatus = await users.LoginUser(email, command.Password);

        if (loginStatus)
        {
            var user = await users.GetUser(email)
                       ?? throw new ApiException(StatusCodes.Status401Unauthorized, "Incorrect email or password");

            return new LoginUserResponse(
                tokenService.GenerateAccessToken(user),
                tokenService.GenerateRefreshToken(user));
        }
    }
}