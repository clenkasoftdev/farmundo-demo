using Farmundo.Demo.Contracts.Chat;

namespace Farmundo.Demo.Application.Abstractions.Chat;

public interface IChatService
{
    Task<MessageDto> SendAsync(string conversationId, string content, string userId, string userName, string email, string role, CancellationToken ct = default);
    Task<IReadOnlyList<MessageDto>> GetConversationAsync(string conversationId, int take = 50, CancellationToken ct = default);
}
