using Farmundo.Demo.Application.Abstractions.Users;
using Farmundo.Demo.Domain.Models;
using Farmundo.Demo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Farmundo.Demo.Infrastructure.Repositories;

public class UserProfileRepository(AppDbContext db) : IUserProfileRepository
{
    public Task<UserProfile?> GetBySubjectAsync(string subject, CancellationToken ct)
        => db.UserProfiles.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                          .FirstOrDefaultAsync(x => x.Subject == subject, ct);

    public Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.UserProfiles.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<UserProfile> UpsertAsync(UserProfile profile, CancellationToken ct)
    {
        var existing = await db.UserProfiles.FirstOrDefaultAsync(x => x.Subject == profile.Subject, ct);
        if (existing is null)
        {
            profile.Id = Guid.NewGuid();
            db.UserProfiles.Add(profile);
        }
        else
        {
            // Preserve existing subscription/IsActive
            profile.Subscription = existing.Subscription;
            profile.IsActive = existing.IsActive;

            existing.UserName = profile.UserName;
            existing.Email = profile.Email;
            existing.RolesCsv = profile.RolesCsv;
            existing.Provider = profile.Provider;
            existing.LastSeenUtc = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return existing ?? profile;
    }

    public async Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct)
    {
        var entity = await db.UserProfiles.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;
        if (entity.IsActive == isActive) return true;
        entity.IsActive = isActive;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(bool Success, bool? NewValue)> ToggleActiveAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.UserProfiles.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return (false, null);
        entity.IsActive = !entity.IsActive;
        await db.SaveChangesAsync(ct);
        return (true, entity.IsActive);
    }
}
