namespace DeveloperProjectTracker.API.DTOs.Labels
{
    public class CreateLabelRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }
    }
}
