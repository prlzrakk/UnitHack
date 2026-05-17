using Infrastructure.Enums;

namespace Infrastructure.Entities;

public class AutomationRule
{
    public Guid Id { get; set; }

    public Guid KanbanId { get; set; }
    public Kanban Kanban { get; set; } = null!;

    public string Name { get; set; } = null!;

    public EventType TriggerType { get; set; }
    public string Condition { get; set; } = null!;
    public string Action { get; set; } = null!;

    public bool IsEnabled { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
