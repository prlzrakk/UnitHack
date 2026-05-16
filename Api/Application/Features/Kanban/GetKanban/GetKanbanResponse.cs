using Api.Application.Features.Tags.Common;
using Infrastructure.Enums;

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
    public List<GetKanbanTaskResponse> Tasks { get; set; } = [];
}

public class GetKanbanTaskResponse
{
    public Guid Id { get; set; }
    public Guid KanbanId { get; set; }
    public Guid ColumnId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime Deadline { get; set; }
    public int Order { get; set; }
    public List<TagResponse> Tags { get; set; } = [];
}
