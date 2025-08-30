using Farmundo.Demo.Application.Abstractions.Persistence;
using Farmundo.Demo.Domain.Models;
using Farmundo.Demo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Farmundo.Demo.Infrastructure.Repositories;

public class ChatRepository(AppDbContext db) : IChatRepository
{
    public async Task<ChatMessage> AddAsync(ChatMessage message, CancellationToken ct)
    {
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync(ct);
        return message;
    }

    public async Task<IReadOnlyList<ChatMessage>> GetRecentAsync(string conversationId, int take, CancellationToken ct)
    {
        return await db.ChatMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }
}
