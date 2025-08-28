using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmundo.Demo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesDemoController : ControllerBase
{
    // Only farmer claim can access
    [Authorize(Policy = "Farmer")]
    [HttpGet("farmer")]
    public IActionResult FarmerOnly()
        => Ok(new { ok = true, role = "farmer", user = User.Identity?.Name });

    // Only buyer claim can access
    [Authorize(Policy = "Buyer")]
    [HttpGet("buyer")]
    public IActionResult BuyerOnly()
        => Ok(new { ok = true, role = "buyer", user = User.Identity?.Name });

    // Only admin claim can access
    [Authorize(Policy = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
        => Ok(new { ok = true, role = "admin", user = User.Identity?.Name });
}
