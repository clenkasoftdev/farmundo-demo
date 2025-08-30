using Farmundo.Demo.Domain.Models;
using Farmundo.Demo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Farmundo.Demo.Api.Authorization
{
    public record SubscriptionRequirement(SubscriptionTier Minimum) : IAuthorizationRequirement;

    public class SubscriptionHandler(AppDbContext db) : AuthorizationHandler<SubscriptionRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SubscriptionRequirement requirement)
        {
            var sub = context.User.FindFirst("sub")?.Value ?? context.User.FindFirst("oid")?.Value;
            if (string.IsNullOrWhiteSpace(sub))
                return;

            //TODO Check from cache first (if enabled)



            var tier = await db.UserProfiles
                .Where(u => u.Subject == sub)
                .Select(u => u.Subscription)
                .FirstOrDefaultAsync(CancellationToken.None);

            if (tier >= requirement.Minimum)
            {
                context.Succeed(requirement);
            }
        }
    }
}
