using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Farmundo.Demo.Api.Security;

// Normalizes incoming claims so authorization policies work regardless of IdP specifics
public class ClaimsMappingTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // C# pattern matching, checking and setting claimsIdentity to id . If you reach code after this, id is non null
        if (principal.Identity is not ClaimsIdentity id || !id.IsAuthenticated)
            return Task.FromResult(principal);

        // Normalize roles -> add standard "role" claims from various sources
        var existingRoleValues = id.FindAll("role").Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

        void AddRole(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !existingRoleValues.Contains(value))
            {
                id.AddClaim(new Claim("role", value));
                existingRoleValues.Add(value);
            }
        }

        // Snapshot enumerations before mutating claims to avoid 'collection was modified'
        var roleTypeValues = id.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        var rolesValues = id.FindAll("roles").Select(c => c.Value).ToArray();
        var groupsValues = id.FindAll("cognito:groups").Select(c => c.Value).ToArray();

        foreach (var r in roleTypeValues) AddRole(r);
        foreach (var r in rolesValues) AddRole(r);
        foreach (var grp in groupsValues)
        {
            foreach (var v in grp.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                AddRole(v);
        }

        // Normalize email -> ensure we have a single 'email' claim if present under other types
        if (id.FindFirst("email") is null)
        {
            var email = id.FindFirst(ClaimTypes.Email)?.Value
                     ?? id.FindFirst("emails")?.Value;
            if (!string.IsNullOrWhiteSpace(email))
            {
                id.AddClaim(new Claim("email", email));
            }
        }

        // Normalize username -> ensure preferred_username exists when possible
        if (id.FindFirst("preferred_username") is null)
        {
            var u = id.FindFirst("cognito:username")?.Value
                 ?? id.FindFirst(ClaimTypes.Name)?.Value
                 ?? id.FindFirst("name")?.Value;
            if (!string.IsNullOrWhiteSpace(u))
            {
                id.AddClaim(new Claim("preferred_username", u));
            }
        }

        return Task.FromResult(principal);
    }
}
