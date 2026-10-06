using DeveloperProjectTracker.API.DTOs.Comments;

namespace DeveloperProjectTracker.API.Services.Comments
{
    public interface ICommentService
    {
        Task<CommentResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid userId,
            CreateCommentRequestDto request);

        Task<List<CommentResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId);

        Task<CommentResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid commentId);

        Task<CommentResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid commentId,
            Guid userId,
            UpdateCommentRequestDto request);

        System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid commentId,
            Guid userId);
    }
}
