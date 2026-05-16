using MediatR;

namespace Api.Application.Features.Teams.AddTeamMember;

public record AddTeamMemberCommand(Guid TeamId, Guid CurrentUserId, Guid UserId)
    : IRequest<AddTeamMemberResponse>;
