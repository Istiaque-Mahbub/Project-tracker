namespace DeveloperProjectTracker.API.DTOs.Milestones
{
    public class MilestoneResponseDto
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
