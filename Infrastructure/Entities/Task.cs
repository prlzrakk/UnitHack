namespace Infrastructure.Entities;

public class Task
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public int Priority { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime Deadline { get; set; }
    
    public Guid ColumnId { get; set; }
    public KanbanColumn Column { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Guid KanbanId { get; set; }
    public Kanban Kanban { get; set; }
    
    public int Order { get; set; }
    
    public List<Tag> Tags { get; set; }
}