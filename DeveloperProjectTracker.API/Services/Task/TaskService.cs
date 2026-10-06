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
            // -----------------------------------------------------
            // Validate tenant
            // -----------------------------------------------------

            var tenantExists = await _context.Tenants
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // -----------------------------------------------------
            // Validate project
            // -----------------------------------------------------

            var projectExists = await _context.Projects
                .AnyAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            // -----------------------------------------------------
            // Validate title
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException(
                    "Task title is required.");
            }

            // -----------------------------------------------------
            // Validate creator
            // -----------------------------------------------------

            var creatorExists = await _context.Users
                .AnyAsync(u => u.Id == userId);

            if (!creatorExists)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            // -----------------------------------------------------
            // Validate creator belongs to tenant
            // -----------------------------------------------------

            var creatorIsTenantMember =
                await _context.TenantMemberships
                    .AnyAsync(x =>
                        x.TenantId == tenantId &&
                        x.UserId == userId);

            if (!creatorIsTenantMember)
            {
                throw new UnauthorizedAccessException(
                    "You are not a member of this tenant.");
            }

            // -----------------------------------------------------
            // Validate assigned user
            // -----------------------------------------------------

            await ValidateAssignedUserAsync(
                tenantId,
                projectId,
                request.AssignedToId);

            // -----------------------------------------------------
            // Validate milestone
            // -----------------------------------------------------

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

            // -----------------------------------------------------
            // Create task
            // -----------------------------------------------------

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),

                ProjectId = projectId,

                Title = request.Title.Trim(),

                Description =
                    request.Description?.Trim(),

                AssignedToId =
                    request.AssignedToId,

                CreatedById =
                    userId,

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

            return await GetByIdAsync(
                tenantId,
                projectId,
                task.Id);
        }

        // =========================================================
        // GET ALL TASKS
        // =========================================================

        public async Task<List<TaskResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId)
        {
            // -----------------------------------------------------
            // Validate tenant
            // -----------------------------------------------------

            var tenantExists = await _context.Tenants
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // -----------------------------------------------------
            // Validate project
            // -----------------------------------------------------

            var projectExists = await _context.Projects
                .AnyAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            // -----------------------------------------------------
            // Get tasks
            // -----------------------------------------------------

            return await _context.TaskItems
                .AsNoTracking()
                .Include(t => t.AssignedTo)
                .Where(t =>
                    t.ProjectId == projectId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,

                    ProjectId = t.ProjectId,

                    Title = t.Title,

                    Description = t.Description,

                    AssignedToId = t.AssignedToId,

                    AssignedToName =
                        t.AssignedTo != null
                            ? t.AssignedTo.FullName
                            : null,

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
            // -----------------------------------------------------
            // Validate tenant
            // -----------------------------------------------------

            var tenantExists = await _context.Tenants
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // -----------------------------------------------------
            // Get task
            // -----------------------------------------------------

            var task = await _context.TaskItems
                .AsNoTracking()
                .Include(t => t.AssignedTo)
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
            // -----------------------------------------------------
            // Validate tenant
            // -----------------------------------------------------

            var tenantExists = await _context.Tenants
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // -----------------------------------------------------
            // Validate project
            // -----------------------------------------------------

            var projectExists = await _context.Projects
                .AnyAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            // -----------------------------------------------------
            // Find task
            // -----------------------------------------------------

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t =>
                    t.Id == taskId &&
                    t.ProjectId == projectId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found.");
            }

            // -----------------------------------------------------
            // Validate title
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException(
                    "Task title is required.");
            }

            // -----------------------------------------------------
            // Validate assigned user
            // -----------------------------------------------------

            await ValidateAssignedUserAsync(
                tenantId,
                projectId,
                request.AssignedToId);

            // -----------------------------------------------------
            // Validate milestone
            // -----------------------------------------------------

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

            // -----------------------------------------------------
            // Update
            // -----------------------------------------------------

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

            return await GetByIdAsync(
                tenantId,
                projectId,
                taskId);
        }

        // =========================================================
        // DELETE TASK
        // =========================================================

        public async System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            // -----------------------------------------------------
            // Validate tenant
            // -----------------------------------------------------

            var tenantExists = await _context.Tenants
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // -----------------------------------------------------
            // Find task
            // -----------------------------------------------------

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

            // -----------------------------------------------------
            // Delete
            // -----------------------------------------------------

            _context.TaskItems.Remove(task);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // ASSIGN TASK
        // =========================================================

        public async Task<TaskResponseDto> AssignAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            AssignTaskRequestDto request)
        {
            // -----------------------------------------------------
            // Validate tenant
            // -----------------------------------------------------

            var tenantExists = await _context.Tenants
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // -----------------------------------------------------
            // Validate project
            // -----------------------------------------------------

            var projectExists = await _context.Projects
                .AnyAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            // -----------------------------------------------------
            // Find task
            // -----------------------------------------------------

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t =>
                    t.Id == taskId &&
                    t.ProjectId == projectId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found.");
            }

            // -----------------------------------------------------
            // Validate assigned user
            // -----------------------------------------------------

            await ValidateAssignedUserAsync(
                tenantId,
                projectId,
                request.AssignedToId);

            // -----------------------------------------------------
            // Assign task
            // -----------------------------------------------------

            task.AssignedToId =
                request.AssignedToId;

            task.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(
                tenantId,
                projectId,
                taskId);
        }

        // =========================================================
        // VALIDATE ASSIGNED USER
        // =========================================================

        private async System.Threading.Tasks.Task ValidateAssignedUserAsync(
            Guid tenantId,
            Guid projectId,
            Guid? assignedToId)
        {
            // -----------------------------------------------------
            // Null means unassigned
            // -----------------------------------------------------

            if (!assignedToId.HasValue)
            {
                return;
            }

            // -----------------------------------------------------
            // User exists
            // -----------------------------------------------------

            var userExists = await _context.Users
                .AnyAsync(u =>
                    u.Id == assignedToId.Value);

            if (!userExists)
            {
                throw new KeyNotFoundException(
                    "Assigned user not found.");
            }

            // -----------------------------------------------------
            // User belongs to tenant
            // -----------------------------------------------------

            var tenantMember =
                await _context.TenantMemberships
                    .AnyAsync(x =>
                        x.TenantId == tenantId &&
                        x.UserId == assignedToId.Value);

            if (!tenantMember)
            {
                throw new ArgumentException(
                    "Assigned user is not a member of this tenant.");
            }

            // -----------------------------------------------------
            // User belongs to project
            // -----------------------------------------------------

            var projectMember =
                await _context.ProjectMembers
                    .AnyAsync(x =>
                        x.ProjectId == projectId &&
                        x.UserId == assignedToId.Value);

            if (!projectMember)
            {
                throw new ArgumentException(
                    "Assigned user is not a member of this project.");
            }
        }

        // =========================================================
        // MAP RESPONSE
        // =========================================================

        private static TaskResponseDto MapToResponse(
            TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,

                ProjectId = task.ProjectId,

                Title = task.Title,

                Description = task.Description,

                AssignedToId = task.AssignedToId,

                AssignedToName =
                    task.AssignedTo != null
                        ? task.AssignedTo.FullName
                        : null,

                CreatedById = task.CreatedById,

                MilestoneId = task.MilestoneId,

                Priority = task.Priority,

                Status = task.Status,

                DueDate = task.DueDate,

                CreatedAt = task.CreatedAt,

                UpdatedAt = task.UpdatedAt
            };
        }
    }
}