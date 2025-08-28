using Farmundo.Demo.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Farmundo.Demo.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ChatMessage>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.ConversationId).IsRequired().HasMaxLength(128);
            b.Property(x => x.SenderUserId).IsRequired().HasMaxLength(256);
            b.Property(x => x.SenderUserName).IsRequired().HasMaxLength(256);
            b.Property(x => x.SenderEmail).IsRequired().HasMaxLength(256);
            b.Property(x => x.Role).IsRequired().HasMaxLength(64);
            b.Property(x => x.Content).IsRequired().HasMaxLength(8000);
            b.Property(x => x.CreatedAt).IsRequired();
            b.HasIndex(x => x.ConversationId);
            b.HasIndex(x => x.CreatedAt);
        });
    }
}
