using DeveloperProjectTracker.API.DTOs.Tenant;

namespace DeveloperProjectTracker.API.Services.Tenant
{
    public interface ITenantMembershipService
    {
        Task<TenantMemberResponseDto> AddMemberAsync(
            Guid tenantId,
            AddTenantMemberRequestDto request);

        Task<List<TenantMemberResponseDto>> GetMembersAsync(
            Guid tenantId);

        Task<TenantMemberResponseDto> GetMemberAsync(
            Guid tenantId,
            Guid userId);

        Task<TenantMemberResponseDto> UpdateRoleAsync(
            Guid tenantId,
            Guid userId,
            UpdateTenantMemberRoleRequestDto request);

       System.Threading.Tasks.Task RemoveMemberAsync(
            Guid tenantId,
            Guid userId);
    
    }
}
