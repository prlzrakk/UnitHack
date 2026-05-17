using Api.Application.Common.Extensions;
using Api.Application.Features.Users.GetMe;
using Api.Application.Features.Users.Register;
using Api.Application.Features.Users.SearchUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Client.Models.DTO.Request;

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
        var result = await mediator.Send(new RegisterUserCommand(req.Email, req.Name ?? string.Empty, req.Password));
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

        return Ok(user);
    }

    /// <summary>
    /// Search users by name or email
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireUserId")]
    public async Task<IActionResult> Search([FromQuery] string query = "", [FromQuery] int limit = 10)
    {
        var users = await mediator.Send(new SearchUsersQuery(query, limit));

        return Ok(users);
    }
}
