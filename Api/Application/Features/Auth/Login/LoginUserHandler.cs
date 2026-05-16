using Api.Application.Common.Exceptions;
using Infrastructure.Interfaces;
using MediatR;
using Shared.Models.DTO.Response;

namespace Api.Application.Features.Auth.Login;

public class LoginUserHandler(IUserRepository users, ITokenService tokenService)
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<LoginUserResponse> Handle(LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        LoginUserValidation.Validate(command);

        var username = command.Email;

        var loginStatus = await users.LoginUser(username, command.Password);

        if (loginStatus)
            return new LoginUserResponse(tokenService.GenerateAccessToken(username),
                tokenService.GenerateRefreshToken(username));
        throw new ApiException(StatusCodes.Status401Unauthorized, "Incorrect password or username");
    }
}