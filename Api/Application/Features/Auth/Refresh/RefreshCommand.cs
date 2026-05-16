using MediatR;
using Shared.Models.DTO.Response;

namespace Api.Application.Features.Auth.Refresh;

public record RefreshCommand(
    string Username
) : IRequest<RefreshResponse>;