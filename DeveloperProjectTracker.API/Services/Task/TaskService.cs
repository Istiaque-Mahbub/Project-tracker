using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Tasks;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;



namespace DeveloperProjectTracker.API.Services.Task
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CREATE TASK
        // =========================================================

        public async Task<TaskResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            Guid userId,
            CreateTaskRequestDto request)
        {
            // Check tenant
            var tenantExists = await _context.Tenants
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // Check project belongs to tenant
            var projectExists = await _context.Projects
                .AnyAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found.");
            }

            // Validate title
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException(
                    "Task title is required.");
            }

            // Check creator exists
            var creatorExists = await _context.Users
                .AnyAsync(u => u.Id == userId);

            if (!creatorExists)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            // Check assigned user
            if (request.AssignedToId.HasValue)
            {
                var assignedUserIsMember =
                    await _context.TenantMemberships
                        .AnyAsync(x =>
                            x.TenantId == tenantId &&
                            x.UserId == request.AssignedToId.Value);

                if (!assignedUserIsMember)
                {
                    throw new ArgumentException(
                        "Assigned user is not a member of this tenant.");
                }
            }

            // Check milestone
            if (request.MilestoneId.HasValue)
            {
                var milestoneExists =
                    await _context.Milestones
                        .AnyAsync(m =>
                            m.Id == request.MilestoneId.Value &&
                            m.ProjectId == projectId);

                if (!milestoneExists)
                {
                    throw new ArgumentException(
                        "Milestone does not belong to this project.");
                }
            }

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),

                ProjectId = projectId,

                Title = request.Title.Trim(),

                Description =
                    request.Description?.Trim(),

                AssignedToId =
                    request.AssignedToId,

                CreatedById = userId,

                MilestoneId =
                    request.MilestoneId,

                Priority =
                    request.Priority,

                Status =
                    request.Status,

                DueDate =
                    request.DueDate,

                CreatedAt =
                    DateTime.UtcNow
            };

            _context.TaskItems.Add(task);

            await _context.SaveChangesAsync();

            return MapToResponse(task);
        }


        // =========================================================
        // GET ALL TASKS
        // =========================================================

        public async Task<List<TaskResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId)
        {
            // Check tenant
            var tenantExists = await _context.Tenants
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // Check project belongs to tenant
            var projectExists = await _context.Projects
                .AnyAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found.");
            }

            return await _context.TaskItems
                .AsNoTracking()
                .Where(t =>
                    t.ProjectId == projectId &&
                    t.Project.TenantId == tenantId)
                .OrderBy(t => t.CreatedAt)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,

                    ProjectId = t.ProjectId,

                    Title = t.Title,

                    Description = t.Description,

                    AssignedToId = t.AssignedToId,

                    CreatedById = t.CreatedById,

                    MilestoneId = t.MilestoneId,

                    Priority = t.Priority,

                    Status = t.Status,

                    DueDate = t.DueDate,

                    CreatedAt = t.CreatedAt,

                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();
        }


        // =========================================================
        // GET SINGLE TASK
        // =========================================================

        public async Task<TaskResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            var task = await _context.TaskItems
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Id == taskId &&
                    t.ProjectId == projectId &&
                    t.Project.TenantId == tenantId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found.");
            }

            return MapToResponse(task);
        }


        // =========================================================
        // UPDATE TASK
        // =========================================================

        public async Task<TaskResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            UpdateTaskRequestDto request)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t =>
                    t.Id == taskId &&
                    t.ProjectId == projectId &&
                    t.Project.TenantId == tenantId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException(
                    "Task title is required.");
            }

            // Validate assigned user
            if (request.AssignedToId.HasValue)
            {
                var assignedUserIsMember =
                    await _context.TenantMemberships
                        .AnyAsync(x =>
                            x.TenantId == tenantId &&
                            x.UserId == request.AssignedToId.Value);

                if (!assignedUserIsMember)
                {
                    throw new ArgumentException(
                        "Assigned user is not a member of this tenant.");
                }
            }

            // Validate milestone
            if (request.MilestoneId.HasValue)
            {
                var milestoneExists =
                    await _context.Milestones
                        .AnyAsync(m =>
                            m.Id == request.MilestoneId.Value &&
                            m.ProjectId == projectId);

                if (!milestoneExists)
                {
                    throw new ArgumentException(
                        "Milestone does not belong to this project.");
                }
            }

            task.Title =
                request.Title.Trim();

            task.Description =
                request.Description?.Trim();

            task.AssignedToId =
                request.AssignedToId;

            task.MilestoneId =
                request.MilestoneId;

            task.Priority =
                request.Priority;

            task.Status =
                request.Status;

            task.DueDate =
                request.DueDate;

            task.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(task);
        }


        // =========================================================
        // DELETE TASK
        // =========================================================

        public async System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t =>
                    t.Id == taskId &&
                    t.ProjectId == projectId &&
                    t.Project.TenantId == tenantId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found.");
            }

            _context.TaskItems.Remove(task);

            await _context.SaveChangesAsync();
        }


        // =========================================================
        // MAPPING
        // =========================================================

        private static TaskResponseDto MapToResponse(
            TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,

                ProjectId =
                    task.ProjectId,

                Title =
                    task.Title,

                Description =
                    task.Description,

                AssignedToId =
                    task.AssignedToId,

                CreatedById =
                    task.CreatedById,

                MilestoneId =
                    task.MilestoneId,

                Priority =
                    task.Priority,

                Status =
                    task.Status,

                DueDate =
                    task.DueDate,

                CreatedAt =
                    task.CreatedAt,

                UpdatedAt =
                    task.UpdatedAt
            };
        }
    }
}
