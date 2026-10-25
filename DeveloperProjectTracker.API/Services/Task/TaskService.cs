using DeveloperProjectTracker.API.Data;
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
            await EnsureActiveTenantAsync(tenantId);

            var projectExists = await _context.Projects.AnyAsync(x =>
                x.Id == projectId && x.TenantId == tenantId);

            if (!projectExists)
                throw new KeyNotFoundException("Project not found in this tenant.");

            await EnsureProjectMemberAsync(projectId, userId, "Task creator");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Task title is required.");

            await ValidateMilestoneAsync(projectId, request.MilestoneId);

            var assignedToIds = NormalizeIds(request.AssignedToIds);
            await ValidateAssigneesAsync(tenantId, projectId, assignedToIds);

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = request.Title.Trim(),
                Description = NormalizeOptionalText(request.Description),
                CreatedById = userId,
                MilestoneId = request.MilestoneId,
                Priority = request.Priority,
                Status = request.Status,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var assignedToId in assignedToIds)
            {
                task.Assignments.Add(new TaskAssignment
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    UserId = assignedToId,
                    AssignedAt = DateTime.UtcNow
                });
            }

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            await _activityLogService.CreateAsync(
                tenantId,
                userId,
                projectId,
                task.Id,
                "TaskCreated",
                $"Task '{task.Title}' was created.");

            foreach (var assignedToId in assignedToIds)
            {
                await _notificationService.CreateAsync(
                    assignedToId,
                    new CreateNotificationRequestDto
                    {
                        Title = "New Task Assigned",
                        Message = $"You have been assigned task '{task.Title}'."
                    });
            }

            return await GetByIdAsync(tenantId, projectId, task.Id);
        }

        // =========================================================
        // GET ALL TASKS
        // =========================================================
        public async Task<List<TaskResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId)
        {
            var projectExists = await _context.Projects.AnyAsync(x =>
                x.Id == projectId && x.TenantId == tenantId);

            if (!projectExists)
                throw new KeyNotFoundException("Project not found in this tenant.");

            var tasks = await _context.TaskItems
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId &&
                            x.Project.TenantId == tenantId)
                .Include(x => x.Assignments)
                    .ThenInclude(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return tasks.Select(MapToResponse).ToList();
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
                .Where(x => x.Id == taskId &&
                            x.ProjectId == projectId &&
                            x.Project.TenantId == tenantId)
                .Include(x => x.Assignments)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync();

            if (task is null)
                throw new KeyNotFoundException("Task not found.");

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
                .Include(x => x.Assignments)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId &&
                    x.Project.TenantId == tenantId);

            if (task is null)
                throw new KeyNotFoundException("Task not found.");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Task title is required.");

            await ValidateMilestoneAsync(projectId, request.MilestoneId);

            var requestedAssigneeIds = NormalizeIds(request.AssignedToIds);
            await ValidateAssigneesAsync(tenantId, projectId, requestedAssigneeIds);

            var previousAssigneeIds = task.Assignments
                .Select(x => x.UserId)
                .ToHashSet();
            var previousStatus = task.Status;
            var previousPriority = task.Priority;
            var previousDueDate = task.DueDate;

            task.Title = request.Title.Trim();
            task.Description = NormalizeOptionalText(request.Description);
            task.MilestoneId = request.MilestoneId;
            task.Priority = request.Priority;
            task.Status = request.Status;
            task.DueDate = request.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            await SyncAssignmentsAsync(task, requestedAssigneeIds);
            await _context.SaveChangesAsync();

            var addedAssigneeIds = requestedAssigneeIds
                .Except(previousAssigneeIds)
                .ToList();
            var removedAssigneeIds = previousAssigneeIds
                .Except(requestedAssigneeIds)
                .ToList();

            if (addedAssigneeIds.Count > 0 || removedAssigneeIds.Count > 0)
            {
                var description =
                    $"Task '{task.Title}' assignments changed. " +
                    $"Added: {FormatIds(addedAssigneeIds)}; " +
                    $"Removed: {FormatIds(removedAssigneeIds)}.";

                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    task.Id,
                    "TaskAssignmentChanged",
                    description);
            }

            if (previousStatus != task.Status)
            {
                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    task.Id,
                    "TaskStatusChanged",
                    $"Task '{task.Title}' status changed from " +
                    $"'{previousStatus}' to '{task.Status}'.");
            }

            if (previousPriority != task.Priority)
            {
                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    task.Id,
                    "TaskPriorityChanged",
                    $"Task '{task.Title}' priority changed from " +
                    $"'{previousPriority}' to '{task.Priority}'.");
            }

            if (previousDueDate != task.DueDate)
            {
                var oldDueDate = previousDueDate.HasValue
                    ? previousDueDate.Value.ToString("dd MMM yyyy")
                    : "No due date";
                var newDueDate = task.DueDate.HasValue
                    ? task.DueDate.Value.ToString("dd MMM yyyy")
                    : "No due date";

                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    task.Id,
                    "TaskDueDateChanged",
                    $"Task '{task.Title}' due date changed from " +
                    $"'{oldDueDate}' to '{newDueDate}'.");
            }

            foreach (var assignedToId in addedAssigneeIds)
            {
                await _notificationService.CreateAsync(
                    assignedToId,
                    new CreateNotificationRequestDto
                    {
                        Title = "Task Assigned",
                        Message = $"You have been assigned task '{task.Title}'."
                    });
            }

            // Notify current assignees of status changes.
            if (previousStatus != task.Status)
            {
                foreach (var assignedToId in requestedAssigneeIds)
                {
                    await _notificationService.CreateAsync(
                        assignedToId,
                        new CreateNotificationRequestDto
                        {
                            Title = "Task Status Updated",
                            Message = $"Task '{task.Title}' status changed to {task.Status}."
                        });
                }
            }

            return await GetByIdAsync(tenantId, projectId, taskId);
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
                .Include(x => x.Assignments)
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId &&
                    x.Project.TenantId == tenantId);

            if (task is null)
                throw new KeyNotFoundException("Task not found.");

            var assignedToIds = task.Assignments
                .Select(x => x.UserId)
                .Distinct()
                .ToList();
            var taskTitle = task.Title;

            await _activityLogService.CreateAsync(
                tenantId,
                userId,
                projectId,
                task.Id,
                "TaskDeleted",
                $"Task '{taskTitle}' was deleted.");

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            foreach (var assignedToId in assignedToIds)
            {
                await _notificationService.CreateAsync(
                    assignedToId,
                    new CreateNotificationRequestDto
                    {
                        Title = "Task Deleted",
                        Message = $"Task '{taskTitle}' has been deleted."
                    });
            }
        }

        // =========================================================
        // ASSIGN TASK (ADD ONE OR MORE ASSIGNEES)
        // =========================================================
        public async Task<TaskResponseDto> AssignAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid userId,
            AssignTaskRequestDto request)
        {
            var task = await _context.TaskItems
                .Include(x => x.Project)
                .Include(x => x.Assignments)
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId &&
                    x.Project.TenantId == tenantId);

            if (task is null)
                throw new KeyNotFoundException("Task not found.");

            await EnsureActiveTenantAsync(tenantId);

            var requestedIds = NormalizeIds(request.AssignedToIds);
            if (requestedIds.Count == 0)
                throw new ArgumentException("At least one assigned user ID is required.");

            await ValidateAssigneesAsync(tenantId, projectId, requestedIds);

            var existingIds = task.Assignments
                .Select(x => x.UserId)
                .ToHashSet();
            var newIds = requestedIds
                .Where(x => !existingIds.Contains(x))
                .ToList();

            foreach (var assignedToId in newIds)
            {
                task.Assignments.Add(new TaskAssignment
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    UserId = assignedToId,
                    AssignedAt = DateTime.UtcNow
                });
            }

            if (newIds.Count > 0)
            {
                task.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _activityLogService.CreateAsync(
                    tenantId,
                    userId,
                    projectId,
                    task.Id,
                    "TaskAssigned",
                    $"Task '{task.Title}' was assigned to user(s): {FormatIds(newIds)}.");

                foreach (var assignedToId in newIds)
                {
                    await _notificationService.CreateAsync(
                        assignedToId,
                        new CreateNotificationRequestDto
                        {
                            Title = "Task Assigned",
                            Message = $"You have been assigned task '{task.Title}'."
                        });
                }
            }

            return await GetByIdAsync(tenantId, projectId, taskId);
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private async System.Threading.Tasks.Task EnsureActiveTenantAsync(Guid tenantId)
        {
            var tenantExists = await _context.Tenants.AnyAsync(x =>
                x.Id == tenantId && x.IsActive);

            if (!tenantExists)
                throw new KeyNotFoundException("Tenant not found or inactive.");
        }

        private async System.Threading.Tasks.Task EnsureProjectMemberAsync(
            Guid projectId,
            Guid userId,
            string userDescription)
        {
            var isMember = await _context.ProjectMembers.AnyAsync(x =>
                x.ProjectId == projectId && x.UserId == userId);

            if (!isMember)
                throw new ArgumentException($"{userDescription} is not a member of this project.");
        }

        private async System.Threading.Tasks.Task ValidateMilestoneAsync(
            Guid projectId,
            Guid? milestoneId)
        {
            if (!milestoneId.HasValue)
                return;

            var milestoneExists = await _context.Milestones.AnyAsync(x =>
                x.Id == milestoneId.Value && x.ProjectId == projectId);

            if (!milestoneExists)
                throw new KeyNotFoundException("Milestone not found in this project.");
        }

        private async System.Threading.Tasks.Task ValidateAssigneesAsync(
            Guid tenantId,
            Guid projectId,
            IReadOnlyCollection<Guid> assignedToIds)
        {
            if (assignedToIds.Count == 0)
                return;

            var existingUserIds = await _context.Users
                .Where(x => assignedToIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            var missingUserIds = assignedToIds.Except(existingUserIds).ToList();
            if (missingUserIds.Count > 0)
                throw new KeyNotFoundException(
                    $"Assigned user(s) not found: {FormatIds(missingUserIds)}.");

            var tenantMemberIds = await _context.TenantMemberships
                .Where(x => x.TenantId == tenantId &&
                            assignedToIds.Contains(x.UserId))
                .Select(x => x.UserId)
                .ToListAsync();

            var nonTenantMembers = assignedToIds.Except(tenantMemberIds).ToList();
            if (nonTenantMembers.Count > 0)
                throw new ArgumentException(
                    $"Assigned user(s) are not members of this tenant: {FormatIds(nonTenantMembers)}.");

            var projectMemberIds = await _context.ProjectMembers
                .Where(x => x.ProjectId == projectId &&
                            assignedToIds.Contains(x.UserId))
                .Select(x => x.UserId)
                .ToListAsync();

            var nonProjectMembers = assignedToIds.Except(projectMemberIds).ToList();
            if (nonProjectMembers.Count > 0)
                throw new ArgumentException(
                    $"Assigned user(s) are not members of this project: {FormatIds(nonProjectMembers)}.");
        }

        private async System.Threading.Tasks.Task SyncAssignmentsAsync(
            TaskItem task,
            IReadOnlyCollection<Guid> requestedIds)
        {
            var requestedSet = requestedIds.ToHashSet();

            var assignmentsToRemove = task.Assignments
                .Where(x => !requestedSet.Contains(x.UserId))
                .ToList();

            _context.Set<TaskAssignment>().RemoveRange(assignmentsToRemove);

            var existingIds = task.Assignments
                .Select(x => x.UserId)
                .ToHashSet();

            foreach (var assignedToId in requestedSet.Where(x => !existingIds.Contains(x)))
            {
                task.Assignments.Add(new TaskAssignment
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    UserId = assignedToId,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        private static List<Guid> NormalizeIds(IEnumerable<Guid>? ids)
        {
            return ids?
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList() ?? new List<Guid>();
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string FormatIds(IEnumerable<Guid> ids)
        {
            var values = ids.Select(x => x.ToString()).ToList();
            return values.Count == 0 ? "none" : string.Join(", ", values);
        }

        private static TaskResponseDto MapToResponse(TaskItem task)
        {
            var assignments = task.Assignments?.ToList()
                ?? new List<TaskAssignment>();

            return new TaskResponseDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                AssignedToIds = assignments
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToList(),
                AssignedToNames = assignments
                    .Where(x => x.User != null)
                    .Select(x => x.User.FullName)
                    .Distinct()
                    .ToList(),
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
