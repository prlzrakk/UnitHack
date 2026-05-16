using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.Features.Example.GetExample;

namespace WebApplication1.Application.Features.Example;

[ApiController]
[Route("api/[controller]")]
public class ExampleController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Example
    /// </summary>
    [HttpGet("get_example")]
    public async Task<IActionResult> GetExample()
    {
        var result = await mediator.Send(new GetExampleQuery());
        return Ok(result);
    }
}