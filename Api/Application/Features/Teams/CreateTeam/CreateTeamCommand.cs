using MediatR;

namespace Api.Application.Features.Teams.CreateTeam;


public record CreateTeamCommand(Guid UserId, string Name) : IRequest<CreateTeamResponse>;
