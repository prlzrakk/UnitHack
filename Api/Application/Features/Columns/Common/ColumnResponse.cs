namespace Api.Application.Features.Columns.Common;

public record ColumnResponse(Guid Id, Guid KanbanId, string Name, int Order);
