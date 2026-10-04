using DeveloperProjectTrackerDomain.Enums;

namespace DeveloperProjectTracker.API.DTOs.Tenant
{
    public class UpdateTenantMemberRoleRequestDto
    {
        public TenantRole Role { get; set; }
    }
}
