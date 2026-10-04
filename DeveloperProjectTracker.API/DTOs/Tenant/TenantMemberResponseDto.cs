using DeveloperProjectTrackerDomain.Enums;

namespace DeveloperProjectTracker.API.DTOs.Tenant
{
    public class TenantMemberResponseDto
    {
        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public TenantRole Role { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
