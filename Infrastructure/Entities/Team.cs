using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Entities;

public class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
}