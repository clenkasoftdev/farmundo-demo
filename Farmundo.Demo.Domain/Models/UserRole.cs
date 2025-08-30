namespace Farmundo.Demo.Domain.Models
{
    public class UserRole
    {
        public Guid UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; } = default!;

        public Guid RoleId { get; set; }
        public Role Role { get; set; } = default!;
    }
}
