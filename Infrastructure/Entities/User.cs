namespace Infrastructure.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string HashPassword { get; set; } = null!;

    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
}
