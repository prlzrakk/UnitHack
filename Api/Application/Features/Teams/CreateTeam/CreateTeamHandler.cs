using Api.Application.Common.Exceptions;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Teams.CreateTeam;

public class CreateTeamHandler(ITeamRepository teams, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTeamCommand, CreateTeamResponse>
{
    public async Task<CreateTeamResponse> Handle(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        if (command.UserId == Guid.Empty)
            throw new BadRequestException("User id is required");
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new BadRequestException("Team name is required");

        var team = await teams.CreateTeam(command.UserId, command.Name.Trim(), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTeamResponse(team.Id, team.Name);
    }
}
