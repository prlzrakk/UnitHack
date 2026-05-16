using Api.Application.Common.Extensions;
using Api.Application.Features.Auth.Login;
using Api.Application.Features.Auth.Refresh;
using Client.Models.DTO.Request;
using Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Application.Features.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create auth session
    /// </summary>
    [HttpPost("sessions")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await mediator.Send(new LoginUserCommand(req.Email, req.Password));

        return Ok(result);
    }

    /// <summary>
    /// Refresh access and refresh token
    /// </summary>
    [HttpPost("tokens/refresh")]
    [Authorize(Policy = AuthPolicies.RefreshTokenOnly)]
    public async Task<IActionResult> Refresh()
    {
        var userId = User.GetUserId();

        var result = await mediator.Send(new RefreshCommand(userId));

        return Ok(result);
    }
}
