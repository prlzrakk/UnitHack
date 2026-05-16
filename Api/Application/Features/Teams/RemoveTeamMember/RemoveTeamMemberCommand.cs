using MediatR;

namespace Api.Application.Features.Teams.RemoveTeamMember;

public record RemoveTeamMemberCommand(Guid TeamId, Guid CurrentUserId, Guid UserId) : IRequest;
