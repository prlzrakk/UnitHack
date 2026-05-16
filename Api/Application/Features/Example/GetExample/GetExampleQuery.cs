using MediatR;

namespace WebApplication1.Application.Features.Example.GetExample;

public record GetExampleQuery : IRequest<string>;