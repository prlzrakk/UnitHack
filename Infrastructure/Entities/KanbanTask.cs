using Infrastructure.Enums;

namespace Infrastructure.Entities;

public class KanbanTask
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    public string? Description { get; set; }
    public Priority Priority { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime Deadline { get; set; }
    
    public Guid ColumnId { get; set; }
    public KanbanColumn Column { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Guid KanbanId { get; set; }
    public Kanban Kanban { get; set; }
    
    public int Order { get; set; }
    
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}