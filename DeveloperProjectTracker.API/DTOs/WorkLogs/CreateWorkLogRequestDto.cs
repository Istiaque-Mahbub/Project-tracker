namespace DeveloperProjectTracker.API.DTOs.WorkLogs
{
    public class CreateWorkLogRequestDto
    {
        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public int? DurationMinutes { get; set; }

        public string? Description { get; set; }
    }
}
