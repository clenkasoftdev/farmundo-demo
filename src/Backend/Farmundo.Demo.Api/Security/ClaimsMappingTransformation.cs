using Farmundo.Demo.Application.Abstractions.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Farmundo.Demo.Api.Security;

// Normalizes incoming claims so authorization policies work regardless of IdP specifics
public class ClaimsMappingTransformation(IServiceScopeFactory scopeFactory, IMemoryCache cache) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity id || !id.IsAuthenticated)
            return principal;

        var existingRoleValues = id.FindAll("role").Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

        void AddRole(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !existingRoleValues.Contains(value))
            {
                id.AddClaim(new Claim("role", value));
                existingRoleValues.Add(value);
            }
        }

        // Snapshot before mutating to avoid 'collection was modified'
        var roleTypeValues = id.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        var rolesValues = id.FindAll("roles").Select(c => c.Value).ToArray();
        var groupsValues = id.FindAll("cognito:groups").Select(c => c.Value).ToArray();

        foreach (var r in roleTypeValues) AddRole(r);
        foreach (var r in rolesValues) AddRole(r);
        foreach (var grp in groupsValues)
            foreach (var v in grp.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                AddRole(v);

        // Augment from local DB roles (union)
        var subject = id.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? id.FindFirst("sub")?.Value ?? id.FindFirst("oid")?.Value;
        if (!string.IsNullOrWhiteSpace(subject))
        {
            var cacheKey = $"userroles:{subject}";
            if (!cache.TryGetValue(cacheKey, out string[]? dbRoles))
            {
                using var scope = scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IUserProfileRepository>();

                var profile = await repo.GetBySubjectAsync(subject, CancellationToken.None);
                dbRoles = profile?.UserRoles.Select(ur => ur.Role.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
                          ?? Array.Empty<string>();

                cache.Set(cacheKey, dbRoles, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            foreach (var role in dbRoles)
                AddRole(role);
        }

        // Normalize email
        if (id.FindFirst("email") is null)
        {
            var email = id.FindFirst(ClaimTypes.Email)?.Value ?? id.FindFirst("emails")?.Value;
            if (!string.IsNullOrWhiteSpace(email))
                id.AddClaim(new Claim("email", email));
        }

        // Normalize username
        if (id.FindFirst("preferred_username") is null)
        {
            var u = id.FindFirst("cognito:username")?.Value
                 ?? id.FindFirst(ClaimTypes.Name)?.Value
                 ?? id.FindFirst("name")?.Value;
            if (!string.IsNullOrWhiteSpace(u))
                id.AddClaim(new Claim("preferred_username", u));
        }

        return principal;
    }
}