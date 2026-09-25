using DeveloperProjectTrackerDomain.Enums;


namespace DeveloperProjectTrackerDomain.Entities
{
    public class TenantMembership
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public Guid UserId { get; set; }

        public TenantRole Role { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
