namespace Infrastructure.Entities;

public class Notification
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid TaskId { get; set; }
    public KanbanTask KanbanTask { get; set; } = null!;
    
    public Guid KanbanId { get; set; }
    public Kanban Kanban { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
