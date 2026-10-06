namespace DeveloperProjectTracker.API.DTOs.ProjectMembers
{
    public class ProjectMemberResponseDto
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
