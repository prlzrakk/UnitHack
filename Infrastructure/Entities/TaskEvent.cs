using Infrastructure.Enums;

namespace Infrastructure.Entities;

public class TaskEvent
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public KanbanTask? Task { get; set; }

    public Guid KanbanId { get; set; }
    public Kanban? Kanban { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public EventType EventType { get; set; }

    public Guid FromColumnId { get; set; }
    public KanbanColumn? FromColumn { get; set; }

    public Guid ToColumnId { get; set; }
    public KanbanColumn? ToColumn { get; set; }

    public int NewOrder { get; set; }
    public int OldOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
