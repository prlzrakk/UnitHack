namespace Infrastructure.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid TeamId { get; set; }
}