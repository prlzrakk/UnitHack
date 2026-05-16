using Client.Models.DTO.Response;
using MediatR;

namespace WebApplication1.Application.Features.Auth.Login;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<LoginUserResponse>;