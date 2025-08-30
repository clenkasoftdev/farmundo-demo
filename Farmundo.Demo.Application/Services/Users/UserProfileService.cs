using System.Security.Claims;
using Farmundo.Demo.Application.Abstractions.Users;
using Farmundo.Demo.Domain.Models;

namespace Farmundo.Demo.Application.Services.Users;

public class UserProfileService(IUserProfileRepository repo) : IUserProfileService
{
    public async Task<UserProfile> UpsertFromClaimsAsync(ClaimsPrincipal user, string provider, CancellationToken ct = default)
    {
        var sub = GetSubject(user);
        var preferredUsername = GetUserName(user);
        var email = GetEmail(user) ?? "unknown@example.com";
        var roles = GetRoles(user);

        var existing = await repo.GetBySubjectAsync(sub, ct);

        var profile = existing ?? new UserProfile
        {
            Id = Guid.NewGuid(),
            Subject = sub,
            Provider = provider,
            FirstSeenUtc = DateTimeOffset.UtcNow,
            Subscription = SubscriptionTier.Standard,
            IsActive = true
        };

        profile.Provider = provider;
        profile.UserName = preferredUsername;
        profile.Email = email;
        profile.RolesCsv = string.Join(',', roles);
        profile.LastSeenUtc = DateTimeOffset.UtcNow;

        return await repo.UpsertAsync(profile, ct);
    }

    public Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => repo.GetByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
        => repo.SetActiveAsync(id, isActive, ct);

    public Task<(bool Success, bool? NewValue)> ToggleActiveAsync(Guid id, CancellationToken ct = default)
        => repo.ToggleActiveAsync(id, ct);

    private static string GetSubject(ClaimsPrincipal user)
        => user.FindFirst("sub")?.Value
           ?? user.FindFirst("oid")?.Value
           ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? user.Identity?.Name
           ?? "unknown";

    private static string GetUserName(ClaimsPrincipal user)
        => user.FindFirst("preferred_username")?.Value
           ?? user.FindFirst("cognito:username")?.Value
           ?? user.FindFirst(ClaimTypes.Name)?.Value
           ?? user.FindFirst("name")?.Value
           ?? user.Identity?.Name
           ?? "unknown";

    private static string? GetEmail(ClaimsPrincipal user)
        => user.FindFirst("email")?.Value
           ?? user.FindFirst(ClaimTypes.Email)?.Value
           ?? user.FindFirst("emails")?.Value;

    private static IEnumerable<string> GetRoles(ClaimsPrincipal user)
    {
        var roles = new List<string>();
        roles.AddRange(user.FindAll("role").Select(c => c.Value));
        roles.AddRange(user.FindAll("roles").Select(c => c.Value));
        roles.AddRange(user.FindAll(ClaimTypes.Role).Select(c => c.Value));
        foreach (var grp in user.FindAll("cognito:groups").Select(c => c.Value))
            foreach (var v in grp.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                roles.Add(v);
        return roles.Distinct(StringComparer.OrdinalIgnoreCase);
    }
}