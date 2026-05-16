namespace Infrastructure.Entities;

public class Notification
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Guid TaskId { get; set; }
    public KanbanTask KanbanTask { get; set; }
    
    public Guid KanbanId { get; set; }
    public Kanban Kanban { get; set; }
    
    public string Message { get; set; }
    
}