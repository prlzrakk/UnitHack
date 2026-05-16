using MediatR;
using Shared.Models.DTO.Response;

namespace Api.Application.Features.Auth.Login;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<LoginUserResponse>;