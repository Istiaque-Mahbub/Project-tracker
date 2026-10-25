using DeveloperProjectTrackerDomain.Enums;
using TaskStatus = DeveloperProjectTrackerDomain.Enums.TaskStatus;


namespace DeveloperProjectTracker.API.DTOs.Tasks
{
    public class TaskResponseDto
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid? AssignedToId { get; set; }

        public Guid CreatedById { get; set; }

        public Guid? MilestoneId { get; set; }

        public TaskPriority Priority { get; set; }

        public TaskStatus Status { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? AssignedToName { get; set; }

        public List<Guid> AssignedToIds { get; set; } = new();

        public List<string> AssignedToNames { get; set; } = new();
    }
}
