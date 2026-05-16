using Infrastructure.Entities;
using Infrastructure.Enums;

namespace Infrastructure.Repositories.Interfaces;

public interface ITeamMemberRepository
{
    Task<bool> IsAdminAsync(Guid teamId, Guid userId, CancellationToken cancellationToken);
    Task<bool> IsMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken);
    Task<TeamMember?> GetMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken);
    Task<List<TeamMember>> GetMembersByTeamIdAsync(Guid teamId, CancellationToken cancellationToken);
    Task<int> CountAdminsAsync(Guid teamId, CancellationToken cancellationToken);
    Task<TeamMember> AddMemberAsync(Guid teamId, Guid userId, TeamRole role, CancellationToken cancellationToken);
    Task<TeamMember?> ChangeRoleAsync(Guid teamId, Guid userId, TeamRole role, CancellationToken cancellationToken);
    Task<bool> RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken);
}
