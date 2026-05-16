namespace Infrastructure.Entities;

public class Kanban
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid ProjectId { get; set; }
}