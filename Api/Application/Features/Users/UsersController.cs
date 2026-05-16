using Api.Application.Common.Extensions;
using Api.Application.Features.Auth.Register;
using Api.Application.Features.Users.GetMe;
using Client.Models.DTO.Request;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>
    /// Get current user
    /// </summary>
    [HttpGet("me")]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.GetUserId();

        var user = await mediator.Send(new GetMeUserQuery(userId));
        return user is null ? NotFound() : Ok(user);
    }
}
