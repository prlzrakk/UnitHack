using Api.Application.Common.Exceptions;
using Client.Models.DTO.Response;
using Infrastructure.Interfaces;
using MediatR;

namespace Api.Application.Features.Auth.Register;

public class RegisterUserHandler(IUserRepository users, IPasswordHasher hasher, ILogger<RegisterUserHandler> logger)
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> Handle(RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        RegisterUserValidation.Validate(command);

        var username = command.Email;

        var hash = hasher.Hash(command.Password);

        var createdUser = await users.RegisterUser(username, hash)
                          ?? throw new ApiException(StatusCodes.Status500InternalServerError,
                              $"User {username} creation failed");

        var name = string.IsNullOrWhiteSpace(command.Name)
            ? null
            : command.Name.Trim();

        if (name is not null && name != createdUser.Name)
        {
            var oldUsername = createdUser.Name;
            await users.ChangeDisplayName(createdUser.Email, name);
            logger.LogInformation("Имя пользователя было измененно с {OldUsername} на {NewUsername}", oldUsername,
                name);
            createdUser.Name = name;
        }

        return new RegisterUserResponse(true);
    }
}