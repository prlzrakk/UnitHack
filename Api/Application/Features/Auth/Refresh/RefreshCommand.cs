using Client.Models.DTO.Response;
using MediatR;

namespace Api.Application.Features.Auth.Refresh;

public record RefreshCommand(
    int UserId
) : IRequest<RefreshResponse>;
