namespace Infrastructure.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string HashPassword { get; set; }
    
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
}