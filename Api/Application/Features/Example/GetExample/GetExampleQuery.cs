using MediatR;

namespace Api.Application.Features.Example.GetExample;

public record GetExampleQuery : IRequest<string>;