using Client.Models.DTO.Request;
using Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.Features.Auth.Login;
using WebApplication1.Application.Features.Auth.Refresh;

namespace WebApplication1.Application.Features.Auth;

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
        var email = User.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var result = await mediator.Send(new RefreshCommand(email));

        return Ok(result);
    }
}
