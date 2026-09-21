using DeveloperProjectTrackerDomain.Enums;


namespace DeveloperProjectTrackerDomain.Entities
{
    public class ProjectMember
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public UserRole Role { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public Project Project { get; set; } = null!;
        public User User { get; set; } = null!;
    
    }
}
