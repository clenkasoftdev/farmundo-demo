using Farmundo.Demo.Application.Abstractions.Chat;
using Farmundo.Demo.Contracts.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmundo.Demo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(IChatService chat) : ControllerBase
{
    [HttpPost("send")] 
    public async Task<ActionResult<MessageDto>> Send([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var user = HttpContext.User;
        var sub = user.FindFirst("sub")?.Value ?? user.FindFirst("oid")?.Value ?? user.Identity?.Name ?? "unknown";
        var preferredUsername = user.FindFirst("preferred_username")?.Value ?? user.FindFirst("cognito:username")?.Value ?? user.Identity?.Name ?? "unknown";
        var email = user.FindFirst("emails")?.Value ?? user.FindFirst("email")?.Value ?? "unknown@example.com";
        var role = user.FindFirst("role")?.Value ?? user.FindFirst("roles")?.Value ?? user.FindFirst("cognito:groups")?.Value?.Split(',').FirstOrDefault() ?? "unknown";

        var dto = await chat.SendAsync(request.ConversationId, request.Content, sub, preferredUsername, email, role, ct);
        return Ok(dto);
    }

    [HttpGet("{conversationId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> Get(string conversationId, [FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(await chat.GetConversationAsync(conversationId, take, ct));
}
