using Api.Application.Common.Exceptions;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Teams.RemoveTeamMember;

public class RemoveTeamMemberHandler(
    ITeamRepository teams,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveTeamMemberCommand>
{
    public async Task Handle(RemoveTeamMemberCommand command, CancellationToken cancellationToken)
    {
        var team = await teams.GetTeam(command.TeamId, cancellationToken)
                   ?? throw new NotFoundException("Team not found");

        var member = await members.GetMemberAsync(team.Id, command.UserId, cancellationToken)
                     ?? throw new NotFoundException("Team member not found");

        var currentUserLeavesTeam = command.CurrentUserId == command.UserId;
        if (!currentUserLeavesTeam)
        {
            var currentUserCanRemove = await members.IsAdminAsync(team.Id, command.CurrentUserId, cancellationToken);
            if (!currentUserCanRemove)
                throw new ForbiddenException("Only team admin can remove members");
        }

        if (member.Role == TeamRole.Admin)
        {
            var adminsCount = await members.CountAdminsAsync(team.Id, cancellationToken);
            if (adminsCount <= 1)
                throw new BadRequestException("Team must have at least one admin");
        }

        await members.RemoveMemberAsync(team.Id, command.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
