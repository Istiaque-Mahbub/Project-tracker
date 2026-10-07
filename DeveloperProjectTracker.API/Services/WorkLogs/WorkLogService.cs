using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.WorkLogs;
using DeveloperProjectTracker.API.Services.ActivityLogs;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.WorkLogs
{
    public class WorkLogService : IWorkLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IActivityLogService _activityLogService;

        public WorkLogService(
            ApplicationDbContext context,
            IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;
        }

        // =========================================================
        // CREATE WORK LOG
        // =========================================================

        public async Task<WorkLogResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid userId,
            CreateWorkLogRequestDto request)
        {
            var (project, task) = await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            // -----------------------------------------------------
            // Validate user
            // -----------------------------------------------------

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            // -----------------------------------------------------
            // Validate dates
            // -----------------------------------------------------

            if (request.EndedAt.HasValue &&
                request.EndedAt.Value < request.StartedAt)
            {
                throw new ArgumentException(
                    "EndedAt cannot be earlier than StartedAt.");
            }

            if (request.DurationMinutes.HasValue &&
                request.DurationMinutes.Value < 0)
            {
                throw new ArgumentException(
                    "DurationMinutes cannot be negative.");
            }

            // -----------------------------------------------------
            // Automatically calculate duration
            // -----------------------------------------------------

            int? durationMinutes =
                request.DurationMinutes;

            if (!durationMinutes.HasValue &&
                request.EndedAt.HasValue)
            {
                durationMinutes =
                    (int)(
                        request.EndedAt.Value -
                        request.StartedAt)
                    .TotalMinutes;
            }

            var workLog = new WorkLog
            {
                Id = Guid.NewGuid(),

                TaskId = taskId,

                UserId = userId,

                StartedAt = request.StartedAt,

                EndedAt = request.EndedAt,

                DurationMinutes = durationMinutes,

                Description = request.Description?.Trim()
            };

            _context.Worklogs.Add(workLog);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                userId,
                projectId,
                taskId,
                "WorkLogCreated",
                $"User '{user.FullName}' created a work log for task '{task.Title}' in project '{project.Name}'.");

            // -----------------------------------------------------
            // Load user for response
            // -----------------------------------------------------

            workLog.User = user;

            return MapToResponse(workLog);
        }

        // =========================================================
        // GET ALL WORK LOGS
        // =========================================================

        public async Task<List<WorkLogResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            return await _context.Worklogs
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .Include(x => x.User)
                .OrderByDescending(x => x.StartedAt)
                .Select(x => new WorkLogResponseDto
                {
                    Id = x.Id,

                    TaskId = x.TaskId,

                    UserId = x.UserId,

                    UserName = x.User.FullName,

                    StartedAt = x.StartedAt,

                    EndedAt = x.EndedAt,

                    DurationMinutes = x.DurationMinutes,

                    Description = x.Description
                })
                .ToListAsync();
        }

        // =========================================================
        // GET SINGLE WORK LOG
        // =========================================================

        public async Task<WorkLogResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid workLogId)
        {
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            var workLog = await _context.Worklogs
                .AsNoTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Id == workLogId &&
                    x.TaskId == taskId);

            if (workLog is null)
            {
                throw new KeyNotFoundException(
                    "Work log not found.");
            }

            return MapToResponse(workLog);
        }

        // =========================================================
        // UPDATE WORK LOG
        // =========================================================

        public async Task<WorkLogResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid workLogId,
            Guid userId,
            UpdateWorkLogRequestDto request)
        {
            var (project, task) = await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            var workLog = await _context.Worklogs
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Id == workLogId &&
                    x.TaskId == taskId);

            if (workLog is null)
            {
                throw new KeyNotFoundException(
                    "Work log not found.");
            }

            // -----------------------------------------------------
            // Only owner can update
            // -----------------------------------------------------

            if (workLog.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can only update your own work logs.");
            }

            // -----------------------------------------------------
            // Store old values
            // -----------------------------------------------------

            var oldStartedAt =
                workLog.StartedAt;

            var oldEndedAt =
                workLog.EndedAt;

            var oldDurationMinutes =
                workLog.DurationMinutes;

            var oldDescription =
                workLog.Description;

            // -----------------------------------------------------
            // Validate dates
            // -----------------------------------------------------

            if (request.EndedAt.HasValue &&
                request.EndedAt.Value < request.StartedAt)
            {
                throw new ArgumentException(
                    "EndedAt cannot be earlier than StartedAt.");
            }

            if (request.DurationMinutes.HasValue &&
                request.DurationMinutes.Value < 0)
            {
                throw new ArgumentException(
                    "DurationMinutes cannot be negative.");
            }

            // -----------------------------------------------------
            // Calculate duration
            // -----------------------------------------------------

            int? durationMinutes =
                request.DurationMinutes;

            if (!durationMinutes.HasValue &&
                request.EndedAt.HasValue)
            {
                durationMinutes =
                    (int)(
                        request.EndedAt.Value -
                        request.StartedAt)
                    .TotalMinutes;
            }

            // -----------------------------------------------------
            // Update
            // -----------------------------------------------------

            workLog.StartedAt =
                request.StartedAt;

            workLog.EndedAt =
                request.EndedAt;

            workLog.DurationMinutes =
                durationMinutes;

            workLog.Description =
                request.Description?.Trim();

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Build activity changes
            // -----------------------------------------------------

            var changes = new List<string>();

            if (oldStartedAt != workLog.StartedAt)
            {
                changes.Add(
                    $"StartedAt changed from '{oldStartedAt}' to '{workLog.StartedAt}'");
            }

            if (oldEndedAt != workLog.EndedAt)
            {
                changes.Add(
                    $"EndedAt changed from '{oldEndedAt}' to '{workLog.EndedAt}'");
            }

            if (oldDurationMinutes != workLog.DurationMinutes)
            {
                changes.Add(
                    $"DurationMinutes changed from '{oldDurationMinutes}' to '{workLog.DurationMinutes}'");
            }

            if (!string.Equals(
                    oldDescription,
                    workLog.Description,
                    StringComparison.Ordinal))
            {
                changes.Add(
                    "Description changed");
            }

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            if (changes.Count > 0)
            {
                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    taskId,
                    "WorkLogUpdated",
                    $"User '{workLog.User.FullName}' updated a work log for task '{task.Title}' in project '{project.Name}'. Changes: {string.Join(", ", changes)}.");
            }

            return MapToResponse(workLog);
        }

        // =========================================================
        // DELETE WORK LOG
        // =========================================================

        public async System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid workLogId,
            Guid userId)
        {
            var (project, task) = await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            var workLog = await _context.Worklogs
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Id == workLogId &&
                    x.TaskId == taskId);

            if (workLog is null)
            {
                throw new KeyNotFoundException(
                    "Work log not found.");
            }

            // -----------------------------------------------------
            // Only owner can delete
            // -----------------------------------------------------

            if (workLog.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can only delete your own work logs.");
            }

            // -----------------------------------------------------
            // Activity Log BEFORE deletion
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                userId,
                projectId,
                taskId,
                "WorkLogDeleted",
                $"User '{workLog.User.FullName}' deleted a work log for task '{task.Title}' in project '{project.Name}'.");

            // -----------------------------------------------------
            // Delete
            // -----------------------------------------------------

            _context.Worklogs.Remove(workLog);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // VALIDATE TASK
        // =========================================================

        private async Task<(Project Project, TaskItem Task)>
            ValidateTaskAsync(
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
            // Get project
            // -----------------------------------------------------

            var project = await _context.Projects
                .FirstOrDefaultAsync(x =>
                    x.Id == projectId &&
                    x.TenantId == tenantId);

            if (project is null)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            // -----------------------------------------------------
            // Get task
            // -----------------------------------------------------

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found in this project.");
            }

            return (project, task);
        }

        // =========================================================
        // MAP RESPONSE
        // =========================================================

        private static WorkLogResponseDto MapToResponse(
            WorkLog workLog)
        {
            return new WorkLogResponseDto
            {
                Id = workLog.Id,

                TaskId = workLog.TaskId,

                UserId = workLog.UserId,

                UserName = workLog.User.FullName,

                StartedAt = workLog.StartedAt,

                EndedAt = workLog.EndedAt,

                DurationMinutes =
                    workLog.DurationMinutes,

                Description =
                    workLog.Description
            };
        }
    }
}