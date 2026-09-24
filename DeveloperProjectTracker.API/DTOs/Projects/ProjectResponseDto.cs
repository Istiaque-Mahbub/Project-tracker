namespace DeveloperProjectTracker.API.DTOs.Projects
{
    public class ProjectResponseDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid OwnerId { get; set; }

        public string OwnerName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
