namespace Infrastructure.Constants;

public static class KanbanDefaults
{
    public const int OrderStep = 1000;

    public static IReadOnlyList<KanbanColumnTemplate> BasicColumns { get; } =
    [
        new("To Do", OrderStep),
        new("In Progress", OrderStep * 2),
        new("Done", OrderStep * 3)
    ];
}

public sealed record KanbanColumnTemplate(string Name, int Order);
