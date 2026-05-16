namespace Api.Application.Features.Teams.ChangeMemberRole;

using Infrastructure.Enums;

public sealed class ChangeMemberRoleRequest
{
    public TeamRole? Role { get; init; }
}
