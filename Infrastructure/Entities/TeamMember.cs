namespace Infrastructure.Entities;

public class TeamMember
{
    public Guid Id { get; set; }
    
    public Guid TeamId { get; set; }
    public Team Team { get; set; } 
    
    public Guid UserId { get; set; }
    
    public string Role { get; set; }
}