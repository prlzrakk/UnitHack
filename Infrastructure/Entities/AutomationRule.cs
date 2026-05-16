using Infrastructure.Enums;

namespace Infrastructure.Entities;

public class AutomationRule
{
    public Guid Id { get; set; }

    public Guid KanbanId { get; set; }
    public Kanban Kanban { get; set; }

    public string Name { get; set; }

    public EventType TriggerType { get; set; }
    public string Condition { get; set; } //json
    public string Action { get; set; }

    public bool IsEnabled { get; set; }
    
    public DateTime CreatedAt { get; set; }
}