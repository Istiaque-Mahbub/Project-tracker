namespace DeveloperProjectTracker.API.DTOs.ActivityLogs
{
    public class ActivityLogResponseDto
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public Guid? ProjectId { get; set; }

        public Guid? TaskId { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
