using DeveloperProjectTracker.API.DTOs.ActivityLogs;

namespace DeveloperProjectTracker.API.Services.ActivityLogs
{
    public interface IActivityLogService
    {
        Task<ActivityLogResponseDto> CreateAsync(
           Guid tenantId,
           Guid userId,
           Guid? projectId,
           Guid? taskId,
           string action,
           string description);

        Task<List<ActivityLogResponseDto>> GetTenantActivitiesAsync(
            Guid tenantId);

        Task<List<ActivityLogResponseDto>> GetProjectActivitiesAsync(
            Guid tenantId,
            Guid projectId);

        Task<List<ActivityLogResponseDto>> GetTaskActivitiesAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId);
    }
}
