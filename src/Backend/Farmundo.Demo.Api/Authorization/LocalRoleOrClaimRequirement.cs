using Farmundo.Demo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Farmundo.Demo.Api.Authorization
{
    public record LocalRoleOrClaimRequirement(string RoleName) : IAuthorizationRequirement;

    public class LocalRoleOrClaimHandler(AppDbContext db) : AuthorizationHandler<LocalRoleOrClaimRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, LocalRoleOrClaimRequirement requirement)
        {
            var user = context.User;
            var hasClaim = user.FindAll("role").Any(c => string.Equals(c.Value, requirement.RoleName, StringComparison.OrdinalIgnoreCase)) ||
                           user.FindAll("roles").Any(c => string.Equals(c.Value, requirement.RoleName, StringComparison.OrdinalIgnoreCase)) ||
                           user.FindAll(ClaimTypes.Role).Any(c => string.Equals(c.Value, requirement.RoleName, StringComparison.OrdinalIgnoreCase));

            if (hasClaim)
            {
                context.Succeed(requirement);
                return;
            }

            var sub = user.FindFirst("sub")?.Value ?? user.FindFirst("oid")?.Value;
            if (string.IsNullOrWhiteSpace(sub))
                return;

            var hasLocalRole = await db.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.UserProfile)
                .AnyAsync(ur => ur.UserProfile.Subject == sub &&
                                ur.Role.Name == requirement.RoleName,
                          CancellationToken.None);

            if (hasLocalRole)
            {
                context.Succeed(requirement);
            }
        }
    }
}
