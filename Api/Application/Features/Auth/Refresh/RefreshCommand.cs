using Client.Models.DTO.Response;
using MediatR;

namespace WebApplication1.Application.Features.Auth.Refresh;

public record RefreshCommand(
    string Email
) : IRequest<RefreshResponse>;
