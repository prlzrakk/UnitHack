using MediatR;
using Shared.Models.DTO.Response;

namespace Api.Application.Features.Auth.Register;

public record RegisterUserCommand(
    string Email,
    string? Name,
    string Password
) : IRequest<RegisterUserResponse>;