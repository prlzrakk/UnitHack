namespace Infrastructure.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid TeamId { get; set; }
    public Team Team { get; set; }
    
    public ICollection<Kanban> Kanbans { get; set; } = new List<Kanban>();
}