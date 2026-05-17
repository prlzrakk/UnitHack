namespace Infrastructure.Entities;

public class ProcessedMessage
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string Handler { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}