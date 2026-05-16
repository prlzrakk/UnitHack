namespace Api.Application.Features.Teams.ChangeMemberRole;

using Infrastructure.Enums;
using MediatR;

public record ChangeMemberRoleCommand(
    Guid TeamId,
    Guid CurrentUserId,
    Guid UserId,
    TeamRole? Role) : IRequest<ChangeMemberRoleResponse>;
