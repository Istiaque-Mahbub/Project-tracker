


namespace DeveloperProjectTrackerDomain.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties

        public ICollection<TenantMembership> Memberships { get; set; }
            = new List<TenantMembership>();

        public ICollection<Project> Projects { get; set; }
            = new List<Project>();
    }
}
