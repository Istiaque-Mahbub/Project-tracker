using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Labels;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Labels
{
    public class LabelService : ILabelService
    {
        private readonly ApplicationDbContext _context;

        public LabelService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CREATE LABEL
        // =========================================================

        public async Task<LabelResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            CreateLabelRequestDto request)
        {
            await ValidateProjectAsync(
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
            UpdateLabelRequestDto request)
        {
            await ValidateProjectAsync(
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

            return MapToResponse(label);
        }

        // =========================================================
        // DELETE LABEL
        // =========================================================

        public async System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid labelId)
        {
            await ValidateProjectAsync(
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

            // Remove task-label relationships first.
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
        }

        // =========================================================
        // ADD LABEL TO TASK
        // =========================================================

        public async Task<TaskLabelResponseDto> AddToTaskAsync(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid labelId)
        {
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

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
            Guid labelId)
        {
            await ValidateTaskAsync(
                tenantId,
                projectId,
                taskId);

            var taskLabel = await _context.TaskLables
                .FirstOrDefaultAsync(x =>
                    x.TaskId == taskId &&
                    x.LabelId == labelId);

            if (taskLabel is null)
            {
                throw new KeyNotFoundException(
                    "This label is not assigned to the task.");
            }

            _context.TaskLables.Remove(taskLabel);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // VALIDATE PROJECT
        // =========================================================

        private async System.Threading.Tasks.Task ValidateProjectAsync(
            Guid tenantId,
            Guid projectId)
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

            var projectExists = await _context.Projects
                .AnyAsync(x =>
                    x.Id == projectId &&
                    x.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }
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
