namespace Infrastructure.Entities;

public class KanbanColumn
{
    public Guid Id { get; set; }
    
    public Guid KanbanId { get; set; }
    public Kanban Kanban { get; set; }
    
    public string Name { get; set; }
    
    public int Order { get; set; }
    public ICollection<KanbanTask> Tasks { get; set; }
}