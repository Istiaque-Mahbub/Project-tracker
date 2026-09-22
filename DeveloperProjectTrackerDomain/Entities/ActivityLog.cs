

using DeveloperProjectTrackerDomain.Enums;

namespace DeveloperProjectTrackerDomain.Entities
{
    public class ActivityLog
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public Guid UserId { get; set; }

        public ActivityType ActivityType { get; set; }

        public string EntityType { get; set; } = string.Empty;

        public Guid? EntityId { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties

        public Project Project { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
