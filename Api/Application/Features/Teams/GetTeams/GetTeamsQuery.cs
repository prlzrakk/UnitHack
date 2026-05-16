using Api.Application.Features.Teams.Common;
using MediatR;

namespace Api.Application.Features.Teams.GetTeams;

public record GetTeamsQuery(Guid CurrentUserId) : IRequest<List<TeamListItemResponse>>;
