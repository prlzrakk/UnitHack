using Client.Models.DTO.Response;
using MediatR;

namespace Api.Application.Features.Auth.Register;

public record RegisterUserCommand(
    string Email,
    string? Name,
    string Password
) : IRequest<RegisterUserResponse>;