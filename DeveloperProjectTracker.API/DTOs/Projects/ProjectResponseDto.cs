namespace DeveloperProjectTracker.API.DTOs.Projects
{
    public class ProjectResponseDto
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid OwnerId { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }
    }
}
