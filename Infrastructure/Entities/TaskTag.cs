namespace Infrastructure.Entities;

public class TaskTag
{
    public Guid TaskId { get; set; }
    public KanbanTask Task { get; set; } = null!;
    
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
