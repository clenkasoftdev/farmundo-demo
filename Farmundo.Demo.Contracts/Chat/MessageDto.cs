namespace Farmundo.Demo.Contracts.Chat;

public sealed class MessageDto
{
    public Guid Id { get; set; }
    public string ConversationId { get; set; } = default!;
    public string SenderUserId { get; set; } = default!;
    public string SenderUserName { get; set; } = default!;
    public string SenderEmail { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}
