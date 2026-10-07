namespace DeveloperProjectTrackerDomain.Entities
{
    public class ActivityLog
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public Guid? ProjectId { get; set; }

        public Guid? TaskId { get; set; }

        public Guid UserId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;

        public Project? Project { get; set; }

        public TaskItem? Task { get; set; }

        public User User { get; set; } = null!;
    }
}