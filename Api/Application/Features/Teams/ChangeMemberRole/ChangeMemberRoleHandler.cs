namespace Api.Application.Features.Teams.ChangeMemberRole;

using Api.Application.Common.Exceptions;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;

public class ChangeMemberRoleHandler(
    ITeamRepository teams,
    ITeamMemberRepository members,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeMemberRoleCommand, ChangeMemberRoleResponse>
{
    public async Task<ChangeMemberRoleResponse> Handle(
        ChangeMemberRoleCommand command,
        CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty)
            throw new BadRequestException("Team id is required");
        if (command.CurrentUserId == Guid.Empty)
            throw new BadRequestException("Current user id is required");
        if (command.UserId == Guid.Empty)
            throw new BadRequestException("User id is required");
        if (!Enum.IsDefined(command.Role))
            throw new BadRequestException("Role is invalid");

        var team = await teams.GetTeam(command.TeamId, cancellationToken)
                   ?? throw new NotFoundException("Team not found");

        var currentUserCanChangeRoles = await members.IsAdminAsync(team.Id, command.CurrentUserId, cancellationToken);
        if (!currentUserCanChangeRoles)
            throw new ForbiddenException("Only team admin can change member roles");

        var member = await members.GetMemberAsync(team.Id, command.UserId, cancellationToken)
                     ?? throw new NotFoundException("Team member not found");

        if (member.Role == TeamRole.Admin && command.Role == TeamRole.Member)
        {
            var adminsCount = await members.CountAdminsAsync(team.Id, cancellationToken);
            if (adminsCount <= 1)
                throw new BadRequestException("Team must have at least one admin");
        }

        member = await members.ChangeRoleAsync(team.Id, command.UserId, command.Role, cancellationToken)
                 ?? throw new NotFoundException("Team member not found");

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChangeMemberRoleResponse(member.TeamId, member.UserId, member.Role);
    }
}
