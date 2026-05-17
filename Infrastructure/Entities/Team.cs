namespace Infrastructure.Entities;

public class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
