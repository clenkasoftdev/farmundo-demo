using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Farmundo.Demo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CacheDemoController(IDistributedCache cache) : ControllerBase
{
    // Simple distributed cache example: set/get text value
    [HttpPost("set")]
    public async Task<IActionResult> Set([FromQuery] string key, [FromBody] string value, CancellationToken ct)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        await cache.SetStringAsync(key, value, options, ct);
        return Ok(new { ok = true });
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] string key, CancellationToken ct)
    {
        var value = await cache.GetStringAsync(key, ct);
        return Ok(new { key, value });
    }
}
