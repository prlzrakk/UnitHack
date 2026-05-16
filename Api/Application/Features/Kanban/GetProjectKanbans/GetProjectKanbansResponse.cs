namespace Api.Application.Features.Kanban.GetProjectKanbans;

public class GetProjectKanbansResponse
{
    public List<KanbanListItemResponse> Kanbans { get; set; } = [];
}

public class KanbanListItemResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
}
