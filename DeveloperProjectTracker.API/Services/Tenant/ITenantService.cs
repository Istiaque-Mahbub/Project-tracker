using DeveloperProjectTracker.API.DTOs.Tenant;

namespace DeveloperProjectTracker.API.Services.Tenants
{
    public interface ITenantService
    {
        Task<TenantResponseDto> CreateAsync(
            CreateTenantRequestDto request);

        Task<List<TenantResponseDto>> GetAllAsync();

        Task<TenantResponseDto> GetByIdAsync(Guid id);

        Task<TenantResponseDto> UpdateAsync(
            Guid id,
            UpdateTenantRequestDto request);

        System.Threading.Tasks.Task ActivateAsync(Guid id);

        System.Threading.Tasks.Task DeactivateAsync(Guid id);
    }
}
