using DeveloperProjectTracker.API.DTOs.Tasks;

namespace DeveloperProjectTracker.API.Services.Task
{
    public interface ITaskService
    {
        Task<TaskResponseDto> CreateAsync(
           Guid tenantId,
           Guid projectId,
           Guid userId,
           CreateTaskRequestDto request);

        Task<List<TaskResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId);

        Task<TaskResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId);

        Task<TaskResponseDto> UpdateAsync(
     Guid tenantId,
     Guid projectId,
     Guid taskId,
     Guid userId,
     UpdateTaskRequestDto request);

        System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid userId);

        Task<TaskResponseDto> AssignAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid userId,
            AssignTaskRequestDto request);
    }
}
