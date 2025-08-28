using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmundo.Demo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var user = HttpContext.User;
        var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new
        {
            name = user.Identity?.Name,
            email = user.FindFirst("emails")?.Value ?? user.FindFirst("email")?.Value,
            username = user.FindFirst("preferred_username")?.Value ?? user.FindFirst("cognito:username")?.Value,
            roles = user.FindAll("role").Select(c => c.Value).Concat(user.FindAll("roles").Select(c => c.Value)).ToArray(),
            claims
        });
    }
}
