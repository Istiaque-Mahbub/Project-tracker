using DeveloperProjectTrackerDomain.Enums;

namespace DeveloperProjectTracker.API.Services.Tenant
{
    public interface ITenantAuthorizationService
    {
        Task<bool> IsMemberAsync(
            Guid userId,
            Guid tenantId);

        Task<bool> HasRoleAsync(
            Guid userId,
            Guid tenantId,
            params TenantRole[] roles);

        Task<bool> IsTenantAdminAsync(
            Guid userId,
            Guid tenantId);

    }
}
