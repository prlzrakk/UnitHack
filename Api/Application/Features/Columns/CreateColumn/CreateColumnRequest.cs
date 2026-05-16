namespace Api.Application.Features.Columns.CreateColumn;

public sealed class CreateColumnRequest
{
    public string Name { get; init; } = string.Empty;
    public int? Order { get; init; }
}
