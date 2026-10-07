using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.ActivityLogs;
using DeveloperProjectTracker.API.DTOs.Notifications;
using DeveloperProjectTracker.API.DTOs.Tasks;
using DeveloperProjectTracker.API.Services.ActivityLogs;
using DeveloperProjectTracker.API.Services.Notifications;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Task
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IActivityLogService _activityLogService;

        public TaskService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IActivityLogService activityLogService)
        {
            _context = context;
            _notificationService = notificationService;
            _activityLogService = activityLogService;
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
            var tenantExists = await _context.Tenants
                .AnyAsync(x =>
                    x.Id == tenantId &&
                    x.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(x =>
                    x.Id == projectId &&
                    x.TenantId == tenantId);

            if (project is null)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            var creatorTenantMember =
                await _context.TenantMemberships
                    .AnyAsync(x =>
                        x.TenantId == tenantId &&
                        x.UserId == userId);

            if (!creatorTenantMember)
            {
                throw new ArgumentException(
                    "User is not a member of this tenant.");
            }

            var creatorProjectMember =
                await _context.ProjectMembers
                    .AnyAsync(x =>
                        x.ProjectId == projectId &&
                        x.UserId == userId);

            if (!creatorProjectMember)
            {
                throw new ArgumentException(
                    "User is not a member of this project.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException(
                    "Task title is required.");
            }

            if (request.MilestoneId.HasValue)
            {
                var milestoneExists =
                    await _context.Milestones
                        .AnyAsync(x =>
                            x.Id == request.MilestoneId.Value &&
                            x.ProjectId == projectId);

                if (!milestoneExists)
                {
                    throw new KeyNotFoundException(
                        "Milestone not found in this project.");
                }
            }

            if (request.AssignedToId.HasValue)
            {
                var assignedUserExists =
                    await _context.Users
                        .AnyAsync(x =>
                            x.Id == request.AssignedToId.Value);

                if (!assignedUserExists)
                {
                    throw new KeyNotFoundException(
                        "Assigned user not found.");
                }

                var assignedTenantMember =
                    await _context.TenantMemberships
                        .AnyAsync(x =>
                            x.TenantId == tenantId &&
                            x.UserId == request.AssignedToId.Value);

                if (!assignedTenantMember)
                {
                    throw new ArgumentException(
                        "Assigned user is not a member of this tenant.");
                }

                var assignedProjectMember =
                    await _context.ProjectMembers
                        .AnyAsync(x =>
                            x.ProjectId == projectId &&
                            x.UserId == request.AssignedToId.Value);

                if (!assignedProjectMember)
                {
                    throw new ArgumentException(
                        "Assigned user is not a member of this project.");
                }
            }

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = request.Title.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(request.Description)
                        ? null
                        : request.Description.Trim(),

                AssignedToId = request.AssignedToId,
                CreatedById = userId,
                MilestoneId = request.MilestoneId,
                Priority = request.Priority,
                Status = request.Status,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // ACTIVITY LOG
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                userId,
                projectId,
                task.Id,
                "TaskCreated",
                $"Task '{task.Title}' was created.");

            // -----------------------------------------------------
            // NOTIFY ASSIGNEE
            // -----------------------------------------------------

            if (task.AssignedToId.HasValue)
            {
                await _notificationService.CreateAsync(
                    task.AssignedToId.Value,
                    new CreateNotificationRequestDto
                    {
                        Title = "New Task Assigned",
                        Message =
                            $"You have been assigned task '{task.Title}'."
                    });
            }

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
            var projectExists = await _context.Projects
                .AnyAsync(x =>
                    x.Id == projectId &&
                    x.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            return await _context.TaskItems
                .AsNoTracking()
                .Where(x =>
                    x.ProjectId == projectId &&
                    x.Project.TenantId == tenantId)
                .Include(x => x.AssignedTo)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new TaskResponseDto
                {
                    Id = x.Id,
                    ProjectId = x.ProjectId,
                    Title = x.Title,
                    Description = x.Description,
                    AssignedToId = x.AssignedToId,
                    CreatedById = x.CreatedById,
                    MilestoneId = x.MilestoneId,
                    Priority = x.Priority,
                    Status = x.Status,
                    DueDate = x.DueDate,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,

                    AssignedToName =
                        x.AssignedTo != null
                            ? x.AssignedTo.FullName
                            : null
                })
                .ToListAsync();
        }

        // =========================================================
        // GET TASK BY ID
        // =========================================================

        public async Task<TaskResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            var task = await _context.TaskItems
                .AsNoTracking()
                .Include(x => x.AssignedTo)
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId &&
                    x.Project.TenantId == tenantId);

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
            Guid userId,
            UpdateTaskRequestDto request)
        {
            var task = await _context.TaskItems
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId &&
                    x.Project.TenantId == tenantId);

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

            if (request.MilestoneId.HasValue)
            {
                var milestoneExists =
                    await _context.Milestones
                        .AnyAsync(x =>
                            x.Id == request.MilestoneId.Value &&
                            x.ProjectId == projectId);

                if (!milestoneExists)
                {
                    throw new KeyNotFoundException(
                        "Milestone not found in this project.");
                }
            }

            if (request.AssignedToId.HasValue)
            {
                var assignedUserExists =
                    await _context.Users
                        .AnyAsync(x =>
                            x.Id == request.AssignedToId.Value);

                if (!assignedUserExists)
                {
                    throw new KeyNotFoundException(
                        "Assigned user not found.");
                }

                var tenantMember =
                    await _context.TenantMemberships
                        .AnyAsync(x =>
                            x.TenantId == tenantId &&
                            x.UserId == request.AssignedToId.Value);

                if (!tenantMember)
                {
                    throw new ArgumentException(
                        "Assigned user is not a member of this tenant.");
                }

                var projectMember =
                    await _context.ProjectMembers
                        .AnyAsync(x =>
                            x.ProjectId == projectId &&
                            x.UserId == request.AssignedToId.Value);

                if (!projectMember)
                {
                    throw new ArgumentException(
                        "Assigned user is not a member of this project.");
                }
            }

            // -----------------------------------------------------
            // STORE PREVIOUS VALUES
            // -----------------------------------------------------

            var previousAssignedToId = task.AssignedToId;
            var previousStatus = task.Status;
            var previousPriority = task.Priority;
            var previousDueDate = task.DueDate;

            // -----------------------------------------------------
            // UPDATE
            // -----------------------------------------------------

            task.Title = request.Title.Trim();

            task.Description =
                string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim();

            task.AssignedToId = request.AssignedToId;
            task.MilestoneId = request.MilestoneId;
            task.Priority = request.Priority;
            task.Status = request.Status;
            task.DueDate = request.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // ACTIVITY: ASSIGNMENT
            // -----------------------------------------------------

            if (previousAssignedToId != task.AssignedToId)
            {
                string assignmentDescription;

                if (task.AssignedToId.HasValue)
                {
                    assignmentDescription =
                        $"Task '{task.Title}' was assigned to user " +
                        $"{task.AssignedToId}.";
                }
                else
                {
                    assignmentDescription =
                        $"Task '{task.Title}' was unassigned.";
                }

                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    task.Id,
                    "TaskAssignmentChanged",
                    assignmentDescription);
            }

            // -----------------------------------------------------
            // ACTIVITY: STATUS
            // -----------------------------------------------------

            if (previousStatus != task.Status)
            {
                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    task.Id,
                    "TaskStatusChanged",
                    $"Task '{task.Title}' status changed " +
                    $"from '{previousStatus}' to '{task.Status}'.");
            }

            // -----------------------------------------------------
            // ACTIVITY: PRIORITY
            // -----------------------------------------------------

            if (previousPriority != task.Priority)
            {
                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    task.Id,
                    "TaskPriorityChanged",
                    $"Task '{task.Title}' priority changed " +
                    $"from '{previousPriority}' to '{task.Priority}'.");
            }

            // -----------------------------------------------------
            // ACTIVITY: DUE DATE
            // -----------------------------------------------------

            if (previousDueDate != task.DueDate)
            {
                var oldDueDate =
                    previousDueDate.HasValue
                        ? previousDueDate.Value.ToString("dd MMM yyyy")
                        : "No due date";

                var newDueDate =
                    task.DueDate.HasValue
                        ? task.DueDate.Value.ToString("dd MMM yyyy")
                        : "No due date";

                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    task.Id,
                    "TaskDueDateChanged",
                    $"Task '{task.Title}' due date changed " +
                    $"from '{oldDueDate}' to '{newDueDate}'.");
            }

            // -----------------------------------------------------
            // NOTIFICATION: ASSIGNEE
            // -----------------------------------------------------

            if (task.AssignedToId.HasValue &&
                previousAssignedToId != task.AssignedToId)
            {
                await _notificationService.CreateAsync(
                    task.AssignedToId.Value,
                    new CreateNotificationRequestDto
                    {
                        Title = "Task Assigned",
                        Message =
                            $"You have been assigned task '{task.Title}'."
                    });
            }

            // -----------------------------------------------------
            // NOTIFICATION: STATUS
            // -----------------------------------------------------

            if (task.AssignedToId.HasValue &&
                previousStatus != task.Status &&
                previousAssignedToId == task.AssignedToId)
            {
                await _notificationService.CreateAsync(
                    task.AssignedToId.Value,
                    new CreateNotificationRequestDto
                    {
                        Title = "Task Status Updated",
                        Message =
                            $"Task '{task.Title}' status changed to {task.Status}."
                    });
            }

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
            Guid taskId,
            Guid userId)
        {
            var task = await _context.TaskItems
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId &&
                    x.Project.TenantId == tenantId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found.");
            }

            var assignedToId = task.AssignedToId;
            var taskTitle = task.Title;

            // -----------------------------------------------------
            // ACTIVITY LOG BEFORE DELETE
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                userId,
                projectId,
                task.Id,
                "TaskDeleted",
                $"Task '{taskTitle}' was deleted.");

            // -----------------------------------------------------
            // DELETE TASK
            // -----------------------------------------------------

            _context.TaskItems.Remove(task);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // NOTIFY ASSIGNEE
            // -----------------------------------------------------

            if (assignedToId.HasValue)
            {
                await _notificationService.CreateAsync(
                    assignedToId.Value,
                    new CreateNotificationRequestDto
                    {
                        Title = "Task Deleted",
                        Message =
                            $"Task '{taskTitle}' has been deleted."
                    });
            }
        }

        // =========================================================
        // ASSIGN TASK
        // =========================================================

        public async Task<TaskResponseDto> AssignAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid userId,
            AssignTaskRequestDto request)
        {
            if (!request.AssignedToId.HasValue ||
                request.AssignedToId.Value == Guid.Empty)
            {
                throw new ArgumentException(
                    "Assigned user ID is required.");
            }

            Guid assignedToId =
                request.AssignedToId.Value;

            var task = await _context.TaskItems
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId &&
                    x.Project.TenantId == tenantId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found.");
            }

            var tenantExists =
                await _context.Tenants
                    .AnyAsync(x =>
                        x.Id == tenantId &&
                        x.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            var userExists =
                await _context.Users
                    .AnyAsync(x =>
                        x.Id == assignedToId);

            if (!userExists)
            {
                throw new KeyNotFoundException(
                    "Assigned user not found.");
            }

            var tenantMember =
                await _context.TenantMemberships
                    .AnyAsync(x =>
                        x.TenantId == tenantId &&
                        x.UserId == assignedToId);

            if (!tenantMember)
            {
                throw new ArgumentException(
                    "User is not a member of this tenant.");
            }

            var projectMember =
                await _context.ProjectMembers
                    .AnyAsync(x =>
                        x.ProjectId == projectId &&
                        x.UserId == assignedToId);

            if (!projectMember)
            {
                throw new ArgumentException(
                    "User is not a member of this project.");
            }

            var previousAssignedToId =
                task.AssignedToId;

            task.AssignedToId = assignedToId;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // ACTIVITY LOG
            // -----------------------------------------------------

            if (previousAssignedToId != assignedToId)
            {
                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    task.Id,
                    "TaskAssigned",
                    $"Task '{task.Title}' was assigned to user " +
                    $"{assignedToId}.");
            }

            // -----------------------------------------------------
            // NOTIFICATION
            // -----------------------------------------------------

            if (previousAssignedToId != assignedToId)
            {
                await _notificationService.CreateAsync(
                    assignedToId,
                    new CreateNotificationRequestDto
                    {
                        Title = "Task Assigned",
                        Message =
                            $"You have been assigned task '{task.Title}'."
                    });
            }

            return await GetByIdAsync(
                tenantId,
                projectId,
                taskId);
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
                CreatedById = task.CreatedById,
                MilestoneId = task.MilestoneId,
                Priority = task.Priority,
                Status = task.Status,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,

                AssignedToName =
                    task.AssignedTo != null
                        ? task.AssignedTo.FullName
                        : null
            };
        }
    }
}