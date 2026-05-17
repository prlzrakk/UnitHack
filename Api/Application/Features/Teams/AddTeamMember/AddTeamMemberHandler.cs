using Api.Application.Common.Exceptions;
using Api.Application.Features.Notifications.Common;
using Infrastructure.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Api.Application.Features.Teams.AddTeamMember;

public class AddTeamMemberHandler(
    ITeamRepository teams,
    ITeamMemberRepository members,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    INotificationSender notificationSender)
    : IRequestHandler<AddTeamMemberCommand, AddTeamMemberResponse>
{
    public async Task<AddTeamMemberResponse> Handle(
        AddTeamMemberCommand command,
        CancellationToken cancellationToken)
    {
        var team = await teams.GetTeam(command.TeamId, cancellationToken)
                   ?? throw new NotFoundException("Team not found");

        var currentUserCanAddMembers = await members.IsAdminAsync(team.Id, command.CurrentUserId, cancellationToken);
        if (!currentUserCanAddMembers)
            throw new ForbiddenException("Only team admin can add members");

        var user = await users.GetUser(command.UserId);
        if (user is null)
            throw new NotFoundException("User not found");

        var alreadyMember = await members.IsMemberAsync(team.Id, command.UserId, cancellationToken);
        if (alreadyMember)
            throw new BadRequestException("User is already a team member");

        var member = await members.AddMemberAsync(team.Id, command.UserId, TeamRole.Member, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationSender.SendToUserAsync(
            command.UserId,
            new
            {
                id = Guid.NewGuid(),
                userId = command.UserId,
                teamId = team.Id,
                type = "TeamMemberAdded",
                name = "Team Member Added",
                message = $"Вас добавили в команду «{team.Name}»",
                isRead = false,
                createdAt = DateTime.UtcNow
            },
            cancellationToken);

        return new AddTeamMemberResponse(member.TeamId, member.UserId, member.Role);
    }
}
