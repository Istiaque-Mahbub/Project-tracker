using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Comments;
using DeveloperProjectTracker.API.DTOs.Notifications;
using DeveloperProjectTracker.API.Services.Notifications;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Comments
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public CommentService(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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

            // -----------------------------------------------------
            // Check commenting user
            // -----------------------------------------------------

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            // -----------------------------------------------------
            // Get task information
            // -----------------------------------------------------

            var task = await _context.TaskItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found.");
            }

            // -----------------------------------------------------
            // Create comment
            // -----------------------------------------------------

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

            // -----------------------------------------------------
            // Load user navigation
            // -----------------------------------------------------

            await _context.Entry(comment)
                .Reference(x => x.User)
                .LoadAsync();

            // -----------------------------------------------------
            // CREATE NOTIFICATIONS
            // -----------------------------------------------------

            var notificationUserIds = new HashSet<Guid>();

            // Notify task creator
            if (task.CreatedById != Guid.Empty &&
                task.CreatedById != userId)
            {
                notificationUserIds.Add(task.CreatedById);
            }

            // Notify assigned developer
            if (task.AssignedToId.HasValue &&
                task.AssignedToId.Value != Guid.Empty &&
                task.AssignedToId.Value != userId)
            {
                notificationUserIds.Add(
                    task.AssignedToId.Value);
            }

            // -----------------------------------------------------
            // Send notification to each relevant user
            // -----------------------------------------------------

            foreach (var notificationUserId in notificationUserIds)
            {
                await _notificationService.CreateAsync(
                    notificationUserId,
                    new CreateNotificationRequestDto
                    {
                        Title = "New Comment",

                        Message =
                            $"{user.FullName} commented on task '{task.Title}'."
                    });
            }

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

            // Only comment owner can update
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

            // Only comment owner can delete
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