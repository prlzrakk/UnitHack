using Infrastructure.Enums;

namespace Api.Application.Features.Teams.Common;

public record TeamListItemResponse(Guid Id, string Name, TeamRole Role);

public record TeamDetailsResponse(Guid Id, string Name, List<TeamMemberResponse> Members);

public record TeamMemberResponse(Guid UserId, string Name, string Email, TeamRole Role);
