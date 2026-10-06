namespace DeveloperProjectTracker.API.DTOs.WorkLogs
{
    public class UpdateWorkLogRequestDto
    {
        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public int? DurationMinutes { get; set; }

        public string? Description { get; set; }
    }
}
