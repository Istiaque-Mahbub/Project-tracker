namespace DeveloperProjectTracker.API.DTOs.Labels
{
    public class UpdateLabelRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }
    }
}
