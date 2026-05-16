namespace Api.Application.Features.Teams.ChangeMemberRole;

using Infrastructure.Enums;

public record ChangeMemberRoleResponse(Guid TeamId, Guid UserId, TeamRole Role);
