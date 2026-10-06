using DeveloperProjectTracker.API.DTOs.Labels;

namespace DeveloperProjectTracker.API.Services.Labels
{

    public interface ILabelService
    {
        // =========================================================
        // PROJECT LABELS
        // =========================================================

        Task<LabelResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            CreateLabelRequestDto request);

        Task<List<LabelResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId);

        Task<LabelResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid labelId);

        Task<LabelResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid labelId,
            UpdateLabelRequestDto request);

        System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid labelId);

        // =========================================================
        // TASK LABELS
        // =========================================================

        Task<TaskLabelResponseDto> AddToTaskAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid labelId);

        Task<List<TaskLabelResponseDto>> GetTaskLabelsAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId);

        System.Threading.Tasks.Task RemoveFromTaskAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid labelId);
    }
}
