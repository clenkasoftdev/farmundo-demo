namespace Farmundo.Demo.Domain.Models;

public class UserProfile
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = default!; // sub or oid
    public string Provider { get; set; } = default!; // Cognito or AzureB2C
    public string UserName { get; set; } = default!; // preferred_username or cognito:username
    public string Email { get; set; } = default!;
    public string RolesCsv { get; set; } = string.Empty; // legacy: roles joined by comma
    public SubscriptionTier Subscription { get; set; } = SubscriptionTier.Trial;
    public bool IsActive { get; set; } = true; // local enable/disable flag
    public DateTimeOffset FirstSeenUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSeenUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

}