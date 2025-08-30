using Farmundo.Demo.Application.Abstractions.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Farmundo.Demo.Api.Hubs;

[Authorize]
public class ChatHub(IChatService chat) : Hub
{
    public async Task SendMessage(string conversationId, string content)
    {
        var user = Context.User!;
        var sub = user.FindFirst("sub")?.Value ?? user.FindFirst("oid")?.Value ?? user.Identity?.Name ?? "unknown";
        var preferredUsername = user.FindFirst("preferred_username")?.Value ?? user.FindFirst("cognito:username")?.Value ?? user.Identity?.Name ?? "unknown";
        var email = user.FindFirst("emails")?.Value ?? user.FindFirst("email")?.Value ?? "unknown@example.com";
        var role = user.FindFirst("role")?.Value ?? user.FindFirst("roles")?.Value ?? user.FindFirst("cognito:groups")?.Value?.Split(',').FirstOrDefault() ?? "unknown";

        var dto = await chat.SendAsync(conversationId, content, sub, preferredUsername, email, role);
        await Clients.Group(conversationId).SendAsync("message", dto);
    }

    public Task JoinConversation(string conversationId)
        => Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
}
