using Api.Application.Common.Exceptions;
using Client.Models.DTO.Response;
using Infrastructure.Interfaces;
using MediatR;

namespace Api.Application.Features.Auth.Login;

public class LoginUserHandler(IUserRepository users, ITokenService tokenService)
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<LoginUserResponse> Handle(LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        LoginUserValidation.Validate(command);

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

        throw new ApiException(StatusCodes.Status401Unauthorized, "Incorrect email or password");
    }
}
