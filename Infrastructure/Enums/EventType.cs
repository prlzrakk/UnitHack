namespace Infrastructure.Enums;

public enum EventType
{
    TaskCreated,
    TaskUpdated,
    TaskDeleted,
    TaskMoved,
    TaskDeadlineChanged,
    TaskPriorityChanged,
    TaskTagAdded,
    TaskTagDeleted,

    KanbanCreated,
    KanbanUpdated,
    KanbanDeleted,

    ColumnCreated,
    ColumnUpdated,
    ColumnDeleted,

    NotificationCreated,
    NotificationRead,

    AutomationRuleTriggered,

    IncomingTaskReceived,
    IncomingTaskProceeded,
    IncomingTaskFailed,
    IncomingTaskDuplicated
}