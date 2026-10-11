
using DeveloperProjectTrackerDomain.Enums;


namespace DeveloperProjectTrackerDomain.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid? AssignedToId { get; set; }

        public Guid CreatedById { get; set; }

        public Guid? MilestoneId { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Todo;

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        public Project Project { get; set; } = null!;

        public User? AssignedTo { get; set; }

        public User CreatedBy { get; set; } = null!;

        public Milestone? Milestone { get; set; }

        public ICollection<Comment> Comments { get; set; }
            = new List<Comment>();

        public ICollection<TaskLabel> TaskLabels { get; set; }
            = new List<TaskLabel>();

        public ICollection<WorkLog> WorkLogs { get; set; }
            = new List<WorkLog>();

        public ICollection<TaskAssignment> Assignments { get; set; }
    = new List<TaskAssignment>();

    }
}