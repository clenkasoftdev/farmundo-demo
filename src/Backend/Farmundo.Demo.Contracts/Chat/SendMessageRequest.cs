namespace Farmundo.Demo.Contracts.Chat;

public sealed class SendMessageRequest
{
    public string ConversationId { get; set; } = default!;
    public string Content { get; set; } = default!;
}
