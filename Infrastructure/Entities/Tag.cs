namespace Infrastructure.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    
    public Guid KanbanId { get; set; }
    public Kanban Kanban { get; set; } = null!;
    
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
