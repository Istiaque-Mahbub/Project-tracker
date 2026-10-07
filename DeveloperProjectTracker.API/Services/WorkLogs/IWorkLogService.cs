using DeveloperProjectTracker.API.DTOs.WorkLogs;

namespace DeveloperProjectTracker.API.Services.WorkLogs
{
    public interface IWorkLogService
    {
        Task<WorkLogResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid userId,
            CreateWorkLogRequestDto request);

        Task<List<WorkLogResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId);

        Task<WorkLogResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid workLogId);

        Task<WorkLogResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid workLogId,
            Guid userId,
            UpdateWorkLogRequestDto request);

        System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid workLogId,
            Guid userId);
    }
}