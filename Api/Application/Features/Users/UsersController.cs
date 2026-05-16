using Api.Application.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DTO.Request;

namespace Api.Application.Features.Users;

[ApiController]
[Route("api/users")]
public class UsersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RegisterRequest req)
    {
        var result = await mediator.Send(new RegisterUserCommand(req.Email, req.Name, req.Password));
        if (!result.Status)
            return NoContent();

        return Created();
    }
}
