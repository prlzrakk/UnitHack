using Infrastructure.Enums;

namespace Infrastructure.Entities;

public class TeamMember
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } 
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public TeamRole Role { get; set; }
}