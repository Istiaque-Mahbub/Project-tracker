using DeveloperProjectTrackerDomain.Enums;

namespace DeveloperProjectTracker.API.DTOs.Tenant
{
    public class AddTenantMemberRequestDto
    {
        public string Email { get; set; } = string.Empty;

        public TenantRole Role { get; set; }
    }
}
