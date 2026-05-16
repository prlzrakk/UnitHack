using Api.Application.Features.Columns.Common;
using MediatR;

namespace Api.Application.Features.Columns.RenameColumn;

public record RenameColumnCommand(Guid ColumnId, Guid CurrentUserId, string Name) : IRequest<ColumnResponse>;
