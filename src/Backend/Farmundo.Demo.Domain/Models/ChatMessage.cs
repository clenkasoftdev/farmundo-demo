namespace Farmundo.Demo.Domain.Models;

public class ChatMessage
{
    public Guid Id { get; set; }
    public string ConversationId { get; set; } = default!;
    public string SenderUserId { get; set; } = default!; // external IdP sub
    public string SenderUserName { get; set; } = default!; // preferred_username or cognito:username
    public string SenderEmail { get; set; } = default!;
    public string Role { get; set; } = default!; // farmer|buyer|admin as claim value
    public string Content { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
