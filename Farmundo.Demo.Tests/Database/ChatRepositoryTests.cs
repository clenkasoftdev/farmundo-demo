using Farmundo.Demo.Infrastructure.Persistence;
using Farmundo.Demo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Farmundo.Demo.Tests.Database;

[Collection("db")] 
public class ChatRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ChatRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Add_And_GetRecent_Works()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        await using var db = new AppDbContext(options);
        var repo = new ChatRepository(db);

        var msg = new Farmundo.Demo.Domain.Models.ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = "it-chat",
            SenderUserId = "sub-1",
            SenderUserName = "user1",
            SenderEmail = "u1@example.com",
            Role = "farmer",
            Content = "hello",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await repo.AddAsync(msg, CancellationToken.None);
        var recent = await repo.GetRecentAsync("it-chat", 10, CancellationToken.None);

        Assert.Single(recent);
        Assert.Equal("hello", recent[0].Content);
    }
}
