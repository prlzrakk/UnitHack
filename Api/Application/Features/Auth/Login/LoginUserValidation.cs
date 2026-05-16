using Api.Application.Common.Exceptions;

namespace Api.Application.Features.Auth.Login;

public static class LoginUserValidation
{
    public static void Validate(LoginUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username))
            throw new ApiException(StatusCodes.Status400BadRequest, "Username is required");

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new ApiException(StatusCodes.Status400BadRequest, "Password is required");

        if (command.Password.Length < 5)
            throw new ApiException(StatusCodes.Status400BadRequest, "Password is too short");
    }
}