using Api.Application.Features.Teams.Common;
using MediatR;

namespace Api.Application.Features.Teams.GetTeam;

public record GetTeamQuery(Guid TeamId, Guid CurrentUserId) : IRequest<TeamDetailsResponse>;
