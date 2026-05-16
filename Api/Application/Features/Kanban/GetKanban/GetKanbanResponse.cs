namespace Api.Application.Features.Kanban.GetKanban;

public class GetKanbanResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<GetKanbanColumnResponse> Columns { get; set; } = [];
}

public class GetKanbanColumnResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}
