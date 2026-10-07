using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.ActivityLogs;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.ActivityLogs
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActivityLogResponseDto> CreateAsync(
            Guid tenantId,
            Guid userId,
            Guid? projectId,
            Guid? taskId,
            string action,
            string description)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant ID is required.");

            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Activity action is required.");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Activity description is required.");

            var tenantExists = await _context.Tenants
                .AnyAsync(x => x.Id == tenantId && x.IsActive);

            if (!tenantExists)
                throw new KeyNotFoundException("Tenant not found.");

            var userExists = await _context.Users
                .AnyAsync(x => x.Id == userId);

            if (!userExists)
                throw new KeyNotFoundException("User not found.");

            if (projectId.HasValue)
            {
                var projectExists = await _context.Projects
                    .AnyAsync(x =>
                        x.Id == projectId.Value &&
                        x.TenantId == tenantId);

                if (!projectExists)
                    throw new KeyNotFoundException(
                        "Project not found in this tenant.");
            }

            if (taskId.HasValue)
            {
                var taskExists = await _context.TaskItems
                    .Include(x => x.Project)
                    .AnyAsync(x =>
                        x.Id == taskId.Value &&
                        x.Project.TenantId == tenantId &&
                        (!projectId.HasValue ||
                         x.ProjectId == projectId.Value));

                if (!taskExists)
                    throw new KeyNotFoundException(
                        "Task not found in this tenant/project.");
            }

            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProjectId = projectId,
                TaskId = taskId,
                UserId = userId,
                Action = action.Trim(),
                Description = description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(activityLog);

            await _context.SaveChangesAsync();

            return await GetByIdAsync(
                tenantId,
                activityLog.Id);
        }

        public async Task<List<ActivityLogResponseDto>>
            GetTenantActivitiesAsync(Guid tenantId)
        {
            await ValidateTenantAsync(tenantId);

            return await _context.ActivityLogs
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ActivityLogResponseDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    ProjectId = x.ProjectId,
                    TaskId = x.TaskId,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    Action = x.Action,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<ActivityLogResponseDto>>
            GetProjectActivitiesAsync(
                Guid tenantId,
                Guid projectId)
        {
            await ValidateProjectAsync(
                tenantId,
                projectId);

            return await _context.ActivityLogs
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == tenantId &&
                    x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ActivityLogResponseDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    ProjectId = x.ProjectId,
                    TaskId = x.TaskId,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    Action = x.Action,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<ActivityLogResponseDto>>
            GetTaskActivitiesAsync(
                Guid tenantId,
                Guid projectId,
                Guid taskId)
        {
            await ValidateProjectAsync(
                tenantId,
                projectId);

            var taskExists = await _context.TaskItems
                .AnyAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId);

            if (!taskExists)
                throw new KeyNotFoundException(
                    "Task not found.");

            return await _context.ActivityLogs
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == tenantId &&
                    x.ProjectId == projectId &&
                    x.TaskId == taskId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ActivityLogResponseDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    ProjectId = x.ProjectId,
                    TaskId = x.TaskId,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    Action = x.Action,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        private async Task<ActivityLogResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid activityId)
        {
            var activity = await _context.ActivityLogs
                .AsNoTracking()
                .Where(x =>
                    x.Id == activityId &&
                    x.TenantId == tenantId)
                .Select(x => new ActivityLogResponseDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    ProjectId = x.ProjectId,
                    TaskId = x.TaskId,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    Action = x.Action,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (activity is null)
                throw new KeyNotFoundException(
                    "Activity log not found.");

            return activity;
        }

        private async System.Threading.Tasks.Task ValidateTenantAsync(Guid tenantId)
        {
            var exists = await _context.Tenants
                .AnyAsync(x =>
                    x.Id == tenantId &&
                    x.IsActive);

            if (!exists)
                throw new KeyNotFoundException(
                    "Tenant not found.");
        }

        private async System.Threading.Tasks.Task ValidateProjectAsync(
            Guid tenantId,
            Guid projectId)
        {
            await ValidateTenantAsync(tenantId);

            var projectExists = await _context.Projects
                .AnyAsync(x =>
                    x.Id == projectId &&
                    x.TenantId == tenantId);

            if (!projectExists)
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
        }
    }
}
