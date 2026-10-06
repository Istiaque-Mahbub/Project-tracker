namespace DeveloperProjectTracker.API.DTOs.WorkLogs
{
    public class WorkLogResponseDto
    {
        public Guid Id { get; set; }

        public Guid TaskId { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public int? DurationMinutes { get; set; }

        public string? Description { get; set; }
    }
}
