

namespace DeveloperProjectTrackerDomain.Entities
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public User Owner { get; set; } = null!;


        public ICollection<ProjectMember> Members { get; set; }
                = new List<ProjectMember>();

        public ICollection<TaskItem> Tasks { get; set; }
            = new List<TaskItem>();

        public ICollection<Milestone> Milestones { get; set; }
            = new List<Milestone>();

        public ICollection<Label> Labels { get; set; }
            = new List<Label>();

        public ICollection<ActivityLog> Activities { get; set; }
            = new List<ActivityLog>();
    }

}
