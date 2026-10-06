using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Comments;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Comments
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // CREATE COMMENT
        // =========================================================

        public async Task<CommentResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid userId,
            CreateCommentRequestDto request)
        {
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                throw new ArgumentException(
                    "Comment content is required.");
            }

            var userExists = await _context.Users
                .AnyAsync(x => x.Id == userId);

            if (!userExists)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),

                TaskId = taskId,

                UserId = userId,

                Content = request.Content.Trim(),

                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();

            await _context.Entry(comment)
                .Reference(x => x.User)
                .LoadAsync();

            return MapToResponse(comment);
        }

        // =========================================================
        // GET ALL COMMENTS
        // =========================================================

        public async Task<List<CommentResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            return await _context.Comments
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .Include(x => x.User)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new CommentResponseDto
                {
                    Id = x.Id,

                    TaskId = x.TaskId,

                    UserId = x.UserId,

                    UserName = x.User.FullName,

                    Content = x.Content,

                    CreatedAt = x.CreatedAt,

                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();
        }

        // =========================================================
        // GET SINGLE COMMENT
        // =========================================================

        public async Task<CommentResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid commentId)
        {
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            var comment = await _context.Comments
                .AsNoTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Id == commentId &&
                    x.TaskId == taskId);

            if (comment is null)
            {
                throw new KeyNotFoundException(
                    "Comment not found.");
            }

            return MapToResponse(comment);
        }

        // =========================================================
        // UPDATE COMMENT
        // =========================================================

        public async Task<CommentResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid commentId,
            Guid userId,
            UpdateCommentRequestDto request)
        {
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                throw new ArgumentException(
                    "Comment content is required.");
            }

            var comment = await _context.Comments
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Id == commentId &&
                    x.TaskId == taskId);

            if (comment is null)
            {
                throw new KeyNotFoundException(
                    "Comment not found.");
            }

            // Only the comment owner can update it.
            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can only update your own comments.");
            }

            comment.Content =
                request.Content.Trim();

            comment.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(comment);
        }

        // =========================================================
        // DELETE COMMENT
        // =========================================================

        public async System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid commentId,
            Guid userId)
        {
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            var comment = await _context.Comments
                .FirstOrDefaultAsync(x =>
                    x.Id == commentId &&
                    x.TaskId == taskId);

            if (comment is null)
            {
                throw new KeyNotFoundException(
                    "Comment not found.");
            }

            // Only the comment owner can delete it.
            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can only delete your own comments.");
            }

            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // VALIDATE TASK
        // =========================================================

        private async System.Threading.Tasks.Task ValidateTaskAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            // -----------------------------------------------------
            // Check tenant
            // -----------------------------------------------------

            var tenantExists = await _context.Tenants
                .AnyAsync(x =>
                    x.Id == tenantId &&
                    x.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // -----------------------------------------------------
            // Check project
            // -----------------------------------------------------

            var projectExists = await _context.Projects
                .AnyAsync(x =>
                    x.Id == projectId &&
                    x.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            // -----------------------------------------------------
            // Check task
            // -----------------------------------------------------

            var taskExists = await _context.TaskItems
                .AnyAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId);

            if (!taskExists)
            {
                throw new KeyNotFoundException(
                    "Task not found in this project.");
            }
        }

        // =========================================================
        // MAP RESPONSE
        // =========================================================

        private static CommentResponseDto MapToResponse(
            Comment comment)
        {
            return new CommentResponseDto
            {
                Id = comment.Id,

                TaskId = comment.TaskId,

                UserId = comment.UserId,

                UserName = comment.User.FullName,

                Content = comment.Content,

                CreatedAt = comment.CreatedAt,

                UpdatedAt = comment.UpdatedAt
            };
        }

    }
}
