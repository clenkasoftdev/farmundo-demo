using Farmundo.Demo.Application.Abstractions.Chat;
using Farmundo.Demo.Application.Abstractions.Persistence;
using Farmundo.Demo.Contracts.Chat;
using Farmundo.Demo.Domain.Models;

namespace Farmundo.Demo.Application.Services.Chat;

public class ChatService(IChatRepository repo) : IChatService
{
    public async Task<MessageDto> SendAsync(string conversationId, string content, string userId, string userName, string email, string role, CancellationToken ct = default)
    {
        var entity = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Content = content,
            SenderUserId = userId,
            SenderUserName = userName,
            SenderEmail = email,
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var saved = await repo.AddAsync(entity, ct);
        return new MessageDto
        {
            Id = saved.Id,
            ConversationId = saved.ConversationId,
            Content = saved.Content,
            SenderUserId = saved.SenderUserId,
            SenderUserName = saved.SenderUserName,
            SenderEmail = saved.SenderEmail,
            Role = saved.Role,
            CreatedAt = saved.CreatedAt
        };
    }

    public async Task<IReadOnlyList<MessageDto>> GetConversationAsync(string conversationId, int take = 50, CancellationToken ct = default)
    {
        var list = await repo.GetRecentAsync(conversationId, take, ct);
        return list
            .OrderBy(m => m.CreatedAt)
            .Select(saved => new MessageDto
            {
                Id = saved.Id,
                ConversationId = saved.ConversationId,
                Content = saved.Content,
                SenderUserId = saved.SenderUserId,
                SenderUserName = saved.SenderUserName,
                SenderEmail = saved.SenderEmail,
                Role = saved.Role,
                CreatedAt = saved.CreatedAt
            })
            .ToList();
    }
}
