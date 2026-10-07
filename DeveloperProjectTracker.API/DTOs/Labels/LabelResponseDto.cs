namespace DeveloperProjectTracker.API.DTOs.Labels
{
    public class LabelResponseDto
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
