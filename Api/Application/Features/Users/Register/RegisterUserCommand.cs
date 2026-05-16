using Client.Models.DTO.Response;
using MediatR;

namespace WebApplication1.Application.Features.Users.Register;

public record RegisterUserCommand(
    string Email,
    string? Name,
    string Password
) : IRequest<RegisterUserResponse>;