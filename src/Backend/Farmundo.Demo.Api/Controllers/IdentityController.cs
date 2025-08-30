using Farmundo.Demo.Application.Abstractions.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmundo.Demo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController(IUserProfileService profiles, IConfiguration config) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var provider = config.GetValue<string>("Auth:Provider") ?? "Unknown";
        var upserted = await profiles.UpsertFromClaimsAsync(HttpContext.User, provider, ct);

        var user = HttpContext.User;
        var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new
        {
            profile = new
            {
                upserted.Id,
                upserted.Subject,
                upserted.Provider,
                upserted.UserName,
                upserted.Email,
                Roles = upserted.RolesCsv,
                Subscription = upserted.Subscription.ToString(),
                upserted.IsActive,
                upserted.FirstSeenUtc,
                upserted.LastSeenUtc
            },
            claims
        });
    }

    // Set IsActive explicitly (admin only)
    [Authorize(Policy = "Admin")]
    [HttpPut("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, [FromQuery] bool value, CancellationToken ct)
    {
        var ok = await profiles.SetActiveAsync(id, value, ct);
        if (!ok) return NotFound();
        var updated = await profiles.GetByIdAsync(id, ct);
        return Ok(new { id, isActive = updated?.IsActive });
    }

    // Toggle IsActive (admin only)
    [Authorize(Policy = "Admin")]
    [HttpPost("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken ct)
    {
        var (success, newValue) = await profiles.ToggleActiveAsync(id, ct);
        if (!success) return NotFound();
        return Ok(new { id, isActive = newValue });
    }
}
