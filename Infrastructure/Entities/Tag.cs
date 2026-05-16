namespace Infrastructure.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid KanbanId { get; set; }
}