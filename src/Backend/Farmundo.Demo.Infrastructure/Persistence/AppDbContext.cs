using Farmundo.Demo.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Farmundo.Demo.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

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

        modelBuilder.Entity<UserProfile>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Subject).IsUnique();
            b.Property(x => x.Provider).IsRequired().HasMaxLength(64);
            b.Property(x => x.Subject).IsRequired().HasMaxLength(256);
            b.Property(x => x.UserName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Email).IsRequired().HasMaxLength(256);
            b.Property(x => x.RolesCsv).IsRequired().HasMaxLength(1024);
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.FirstSeenUtc).IsRequired();
            b.Property(x => x.LastSeenUtc).IsRequired();

            // Store SubscriptionTier as string
            b.Property(x => x.Subscription)
             .HasConversion<string>()
             .IsRequired();
        });

        modelBuilder.Entity<Role>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Name).IsRequired().HasMaxLength(128);
            b.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<UserRole>(b =>
        {
            b.HasKey(ur => new { ur.UserProfileId, ur.RoleId });
            b.HasOne(ur => ur.UserProfile)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
