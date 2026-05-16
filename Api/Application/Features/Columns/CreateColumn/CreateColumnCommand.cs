using Api.Application.Features.Columns.Common;
using MediatR;

namespace Api.Application.Features.Columns.CreateColumn;

public record CreateColumnCommand(Guid KanbanId, Guid CurrentUserId, string Name, int? Order)
    : IRequest<ColumnResponse>;
