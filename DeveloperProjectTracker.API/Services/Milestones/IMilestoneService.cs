using DeveloperProjectTracker.API.DTOs.Milestones;

namespace DeveloperProjectTracker.API.Services.Milestones
{
    public interface IMilestoneService
    {
        // =========================================================
        // CREATE
        // =========================================================

        Task<MilestoneResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            Guid actorUserId,
            CreateMilestoneRequestDto request);

        // =========================================================
        // GET ALL
        // =========================================================

        Task<List<MilestoneResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId);

        // =========================================================
        // GET BY ID
        // =========================================================

        Task<MilestoneResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid milestoneId);

        // =========================================================
        // UPDATE
        // =========================================================

        Task<MilestoneResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid milestoneId,
            Guid actorUserId,
            UpdateMilestoneRequestDto request);

        // =========================================================
        // DELETE
        // =========================================================

        System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid milestoneId,
            Guid actorUserId);
    }
}