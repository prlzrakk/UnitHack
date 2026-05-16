namespace Api.Application.Features.Kanban.CreateKanban;

public class CreateKanbanResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public List<KanbanColumnResponse> Columns { get; set; } = [];
}

public class KanbanColumnResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}