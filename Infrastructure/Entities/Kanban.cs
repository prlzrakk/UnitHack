namespace Infrastructure.Entities;

public class Kanban
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; }
    
    public ICollection<KanbanColumn> Columns { get; set; } = new List<KanbanColumn>();
    public ICollection<KanbanTask> Tasks { get; set; } = new List<KanbanTask>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
