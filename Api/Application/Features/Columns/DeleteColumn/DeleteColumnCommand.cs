using MediatR;

namespace Api.Application.Features.Columns.DeleteColumn;

public record DeleteColumnCommand(Guid ColumnId, Guid CurrentUserId) : IRequest;
