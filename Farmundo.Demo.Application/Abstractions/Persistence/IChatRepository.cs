using Farmundo.Demo.Domain.Models;

namespace Farmundo.Demo.Application.Abstractions.Persistence;

public interface IChatRepository
{
    Task<ChatMessage> AddAsync(ChatMessage message, CancellationToken ct);
    Task<IReadOnlyList<ChatMessage>> GetRecentAsync(string conversationId, int take, CancellationToken ct);
}
