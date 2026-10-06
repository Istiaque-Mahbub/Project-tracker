using DeveloperProjectTracker.API.DTOs.Milestones;

namespace DeveloperProjectTracker.API.Services.Milestones
{
    public interface IMilestoneService
    {
        Task<MilestoneResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            CreateMilestoneRequestDto request);

        Task<List<MilestoneResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId);

        Task<MilestoneResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid milestoneId);

        Task<MilestoneResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid milestoneId,
            UpdateMilestoneRequestDto request);

        System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid milestoneId);

    }
}
