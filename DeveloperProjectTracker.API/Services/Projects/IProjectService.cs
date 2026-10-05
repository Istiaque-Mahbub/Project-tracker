using DeveloperProjectTracker.API.DTOs.Projects;

namespace DeveloperProjectTracker.API.Services.Projects
{
    public interface IProjectService
    {
        Task<ProjectResponseDto> CreateAsync(
            Guid tenantId,
            CreateProjectRequestDto request);

        Task<List<ProjectResponseDto>> GetAllAsync(
            Guid tenantId);

        Task<ProjectResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId);

        Task<ProjectResponseDto> UpdateAsync(
    Guid tenantId,
    Guid projectId,
    UpdateProjectRequestDto request);

        System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId);
    }
}
