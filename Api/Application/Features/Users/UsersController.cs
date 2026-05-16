using Api.Application.Features.Auth.Register;
using Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DTO.Request;

namespace Api.Application.Features.Users;

[ApiController]
[Route("api/users")]
public class UsersController(IMediator mediator, IUserRepository users) : ControllerBase
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
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await users.GetUser(userId);
        return user is null ? NotFound() : Ok(user);
    }
}
