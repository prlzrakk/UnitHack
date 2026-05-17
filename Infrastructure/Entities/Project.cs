namespace Infrastructure.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public ICollection<Kanban> Kanbans { get; set; } = new List<Kanban>();
}
