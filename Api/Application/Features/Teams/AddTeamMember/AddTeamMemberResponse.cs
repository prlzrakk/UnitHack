using Infrastructure.Enums;

namespace Api.Application.Features.Teams.AddTeamMember;

public record AddTeamMemberResponse(Guid TeamId, Guid UserId, TeamRole Role);
