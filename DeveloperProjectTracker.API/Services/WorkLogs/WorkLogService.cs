using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.WorkLogs;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.WorkLogs
{
    public class WorkLogService : IWorkLogService
    {
        private readonly ApplicationDbContext _context;

        public WorkLogService(
            ApplicationDbContext context)
        {
            _context = context;
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
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            // -----------------------------------------------------
            // Validate user
            // -----------------------------------------------------

            var userExists = await _context.Users
                .AnyAsync(x => x.Id == userId);

            if (!userExists)
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

            await _context.Entry(workLog)
                .Reference(x => x.User)
                .LoadAsync();

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
            await ValidateTaskAsync(
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

            workLog.StartedAt =
                request.StartedAt;

            workLog.EndedAt =
                request.EndedAt;

            workLog.DurationMinutes =
                durationMinutes;

            workLog.Description =
                request.Description?.Trim();

            await _context.SaveChangesAsync();

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
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            var workLog = await _context.Worklogs
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

            _context.Worklogs.Remove(workLog);

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
