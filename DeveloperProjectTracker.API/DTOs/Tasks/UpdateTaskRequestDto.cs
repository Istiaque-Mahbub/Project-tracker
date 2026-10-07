using DeveloperProjectTrackerDomain.Enums;
using TaskStatus = DeveloperProjectTrackerDomain.Enums.TaskStatus;

namespace DeveloperProjectTracker.API.DTOs.Tasks
{
    public class UpdateTaskRequestDto
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid? AssignedToId { get; set; }

        public Guid? MilestoneId { get; set; }

        public TaskPriority Priority { get; set; }

        public TaskStatus Status { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
