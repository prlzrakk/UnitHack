namespace WebApplication1.Application.Features.Kanban.ChangeKanban;

public class ChangeKanbanResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; }
}