using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Labels;
using DeveloperProjectTracker.API.Services.ActivityLogs;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Labels
{
    public class LabelService : ILabelService
    {
        private readonly ApplicationDbContext _context;
        private readonly IActivityLogService _activityLogService;

        public LabelService(
            ApplicationDbContext context,
            IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;
        }

        // =========================================================
        // CREATE LABEL
        // =========================================================

        public async Task<LabelResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            Guid actorUserId,
            CreateLabelRequestDto request)
        {
            var project = await ValidateProjectAsync(
                tenantId,
                projectId);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Label name is required.");
            }

            var labelName = request.Name.Trim();

            // Prevent duplicate label names in the same project.
            var exists = await _context.Labels
                .AnyAsync(x =>
                    x.ProjectId == projectId &&
                    x.Name.ToLower() == labelName.ToLower());

            if (exists)
            {
                throw new InvalidOperationException(
                    "A label with this name already exists in this project.");
            }

            var label = new Label
            {
                Id = Guid.NewGuid(),

                ProjectId = projectId,

                Name = labelName,

                Color = request.Color?.Trim(),

                CreatedAt = DateTime.UtcNow
            };

            _context.Labels.Add(label);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                actorUserId,
                projectId,
                null,
                "LabelCreated",
                $"Label '{label.Name}' was created in project '{project.Name}'.");

            return MapToResponse(label);
        }

        // =========================================================
        // GET ALL PROJECT LABELS
        // =========================================================

        public async Task<List<LabelResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId)
        {
            await ValidateProjectAsync(
                tenantId,
                projectId);

            return await _context.Labels
                .AsNoTracking()
                .Where(x =>
                    x.ProjectId == projectId)
                .OrderBy(x => x.Name)
                .Select(x => new LabelResponseDto
                {
                    Id = x.Id,

                    ProjectId = x.ProjectId,

                    Name = x.Name,

                    Color = x.Color,

                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        // =========================================================
        // GET SINGLE LABEL
        // =========================================================

        public async Task<LabelResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid labelId)
        {
            await ValidateProjectAsync(
                tenantId,
                projectId);

            var label = await _context.Labels
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == labelId &&
                    x.ProjectId == projectId);

            if (label is null)
            {
                throw new KeyNotFoundException(
                    "Label not found.");
            }

            return MapToResponse(label);
        }

        // =========================================================
        // UPDATE LABEL
        // =========================================================

        public async Task<LabelResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid labelId,
            Guid actorUserId,
            UpdateLabelRequestDto request)
        {
            var project = await ValidateProjectAsync(
                tenantId,
                projectId);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Label name is required.");
            }

            var label = await _context.Labels
                .FirstOrDefaultAsync(x =>
                    x.Id == labelId &&
                    x.ProjectId == projectId);

            if (label is null)
            {
                throw new KeyNotFoundException(
                    "Label not found.");
            }

            // -----------------------------------------------------
            // Store old values for activity log
            // -----------------------------------------------------

            var oldName = label.Name;
            var oldColor = label.Color;

            var labelName = request.Name.Trim();

            var duplicateExists = await _context.Labels
                .AnyAsync(x =>
                    x.Id != labelId &&
                    x.ProjectId == projectId &&
                    x.Name.ToLower() == labelName.ToLower());

            if (duplicateExists)
            {
                throw new InvalidOperationException(
                    "A label with this name already exists in this project.");
            }

            label.Name = labelName;

            label.Color = request.Color?.Trim();

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Build activity description
            // -----------------------------------------------------

            var changes = new List<string>();

            if (oldName != label.Name)
            {
                changes.Add(
                    $"name changed from '{oldName}' to '{label.Name}'");
            }

            if (oldColor != label.Color)
            {
                changes.Add("color updated");
            }

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            if (changes.Count > 0)
            {
                await _activityLogService.CreateAsync(
                    tenantId,
                    actorUserId,
                    projectId,
                    null,
                    "LabelUpdated",
                    $"Label '{label.Name}' was updated in project '{project.Name}'. " +
                    $"Changes: {string.Join("; ", changes)}.");
            }

            return MapToResponse(label);
        }

        // =========================================================
        // DELETE LABEL
        // =========================================================

        public async System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid labelId,
            Guid actorUserId)
        {
            var project = await ValidateProjectAsync(
                tenantId,
                projectId);

            var label = await _context.Labels
                .FirstOrDefaultAsync(x =>
                    x.Id == labelId &&
                    x.ProjectId == projectId);

            if (label is null)
            {
                throw new KeyNotFoundException(
                    "Label not found.");
            }

            // -----------------------------------------------------
            // Store label name before deletion
            // -----------------------------------------------------

            var labelName = label.Name;

            // -----------------------------------------------------
            // Remove task-label relationships first
            // -----------------------------------------------------

            var taskLabels = await _context.TaskLables
                .Where(x =>
                    x.LabelId == labelId)
                .ToListAsync();

            if (taskLabels.Count > 0)
            {
                _context.TaskLables.RemoveRange(
                    taskLabels);
            }

            _context.Labels.Remove(label);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                actorUserId,
                projectId,
                null,
                "LabelDeleted",
                $"Label '{labelName}' was deleted from project '{project.Name}'.");
        }

        // =========================================================
        // ADD LABEL TO TASK
        // =========================================================

        public async Task<TaskLabelResponseDto> AddToTaskAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid labelId,
            Guid actorUserId)
        {
            var project = await ValidateProjectAsync(
                tenantId,
                projectId);

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found in this project.");
            }

            var label = await _context.Labels
                .FirstOrDefaultAsync(x =>
                    x.Id == labelId &&
                    x.ProjectId == projectId);

            if (label is null)
            {
                throw new KeyNotFoundException(
                    "Label not found in this project.");
            }

            var alreadyAssigned = await _context.TaskLables
                .AnyAsync(x =>
                    x.TaskId == taskId &&
                    x.LabelId == labelId);

            if (alreadyAssigned)
            {
                throw new InvalidOperationException(
                    "This label is already assigned to the task.");
            }

            var taskLabel = new TaskLabel
            {
                TaskId = taskId,

                LabelId = labelId
            };

            _context.TaskLables.Add(taskLabel);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                actorUserId,
                projectId,
                taskId,
                "TaskLabelAdded",
                $"Label '{label.Name}' was added to task '{task.Title}' in project '{project.Name}'.");

            return new TaskLabelResponseDto
            {
                TaskId = taskId,

                LabelId = label.Id,

                LabelName = label.Name,

                Color = label.Color
            };
        }

        // =========================================================
        // GET TASK LABELS
        // =========================================================

        public async Task<List<TaskLabelResponseDto>> GetTaskLabelsAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            return await _context.TaskLables
                .AsNoTracking()
                .Where(x =>
                    x.TaskId == taskId)
                .Include(x => x.Label)
                .OrderBy(x => x.Label.Name)
                .Select(x => new TaskLabelResponseDto
                {
                    TaskId = x.TaskId,

                    LabelId = x.LabelId,

                    LabelName = x.Label.Name,

                    Color = x.Label.Color
                })
                .ToListAsync();
        }

        // =========================================================
        // REMOVE LABEL FROM TASK
        // =========================================================

        public async System.Threading.Tasks.Task RemoveFromTaskAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid labelId,
            Guid actorUserId)
        {
            var project = await ValidateProjectAsync(
                tenantId,
                projectId);

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId &&
                    x.ProjectId == projectId);

            if (task is null)
            {
                throw new KeyNotFoundException(
                    "Task not found in this project.");
            }

            var taskLabel = await _context.TaskLables
                .Include(x => x.Label)
                .FirstOrDefaultAsync(x =>
                    x.TaskId == taskId &&
                    x.LabelId == labelId);

            if (taskLabel is null)
            {
                throw new KeyNotFoundException(
                    "This label is not assigned to the task.");
            }

            var labelName = taskLabel.Label.Name;

            _context.TaskLables.Remove(taskLabel);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                actorUserId,
                projectId,
                taskId,
                "TaskLabelRemoved",
                $"Label '{labelName}' was removed from task '{task.Title}' in project '{project.Name}'.");
        }

        // =========================================================
        // VALIDATE PROJECT
        // =========================================================

        private async Task<DeveloperProjectTrackerDomain.Entities.Project>
            ValidateProjectAsync(
                Guid tenantId,
                Guid projectId)
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

            var project = await _context.Projects
                .FirstOrDefaultAsync(x =>
                    x.Id == projectId &&
                    x.TenantId == tenantId);

            if (project is null)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            return project;
        }

        // =========================================================
        // VALIDATE TASK
        // =========================================================

        private async System.Threading.Tasks.Task ValidateTaskAsync(
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
            {
                throw new KeyNotFoundException(
                    "Task not found in this project.");
            }
        }

        // =========================================================
        // MAP LABEL
        // =========================================================

        private static LabelResponseDto MapToResponse(
            Label label)
        {
            return new LabelResponseDto
            {
                Id = label.Id,

                ProjectId = label.ProjectId,

                Name = label.Name,

                Color = label.Color,

                CreatedAt = label.CreatedAt
            };
        }
    }
}