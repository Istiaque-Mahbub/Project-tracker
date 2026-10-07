using DeveloperProjectTracker.API.DTOs.ProjectMembers;

namespace DeveloperProjectTracker.API.Services.ProjectMembers
{
    public interface IProjectMemberService
    {
        Task<ProjectMemberResponseDto> AddMemberAsync(
            Guid tenantId,
            Guid projectId,
            Guid userId,
            AddProjectMemberRequestDto request);

        Task<List<ProjectMemberResponseDto>> GetMembersAsync(
            Guid tenantId,
            Guid projectId);

        Task<ProjectMemberResponseDto> GetMemberAsync(
            Guid tenantId,
            Guid projectId,
            Guid userId);

        System.Threading.Tasks.Task RemoveMemberAsync(
            Guid tenantId,
            Guid projectId,
            Guid userId,
            Guid actorUserId);
    }
}