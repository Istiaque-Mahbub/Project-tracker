namespace DeveloperProjectTracker.API.DTOs.Labels
{
    public class TaskLabelResponseDto
    {
        public Guid TaskId { get; set; }

        public Guid LabelId { get; set; }

        public string LabelName { get; set; } = string.Empty;

        public string? Color { get; set; }
    }
}
