using MediatR;

namespace WebApplication1.Application.Features.Example.GetExample;

public class GetExampleHandler : IRequestHandler<GetExampleQuery, string>
{
    public Task<string> Handle(GetExampleQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult("EXAMPLEEEE");
    }
}