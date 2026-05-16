using Api.Application.Common.Exceptions;

namespace Api.Application.Features.Auth.Register;

public static class RegisterUserValidation
{
    public static void Validate(RegisterUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ApiException(StatusCodes.Status400BadRequest, "Email is required");

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new ApiException(StatusCodes.Status400BadRequest, "Password is required");

        if (command.Password.Length < 5)
            throw new ApiException(StatusCodes.Status400BadRequest, "Password is too short");
    }
}
