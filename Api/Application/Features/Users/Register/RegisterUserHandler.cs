using Api.Application.Common.Exceptions;
using Client.Models.DTO.Response;

using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security.Interfaces;


using MediatR;

namespace Api.Application.Features.Users.Register;

public class RegisterUserHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> Handle(RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var username = command.Email;

        var hash = hasher.Hash(command.Password);
        var name = command.Name.Trim();
        var createdUser = await users.RegisterUser(username, name, hash)
                          ?? throw new BadRequestException($"User {username} already exists");

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse(true);
    }
}
