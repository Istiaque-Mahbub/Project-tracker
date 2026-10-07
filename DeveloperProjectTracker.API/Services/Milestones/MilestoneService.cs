using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Milestones;
using DeveloperProjectTracker.API.DTOs.Notifications;
using DeveloperProjectTracker.API.Services.ActivityLogs;
using DeveloperProjectTracker.API.Services.Notifications;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Milestones
{
    public class MilestoneService : IMilestoneService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IActivityLogService _activityLogService;

        public MilestoneService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IActivityLogService activityLogService)
        {
            _context = context;
            _notificationService = notificationService;
            _activityLogService = activityLogService;
        }

        // =========================================================
        // CREATE
        // =========================================================

        public async Task<MilestoneResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            Guid actorUserId,
            CreateMilestoneRequestDto request)
        {
            var project = await ValidateProjectAsync(
                tenantId,
                projectId);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Milestone name is required.");
            }

            if (request.StartDate > request.DueDate)
            {
                throw new ArgumentException(
                    "Start date cannot be later than due date.");
            }

            var milestone = new Milestone
            {
                Id = Guid.NewGuid(),

                ProjectId = projectId,

                Name = request.Name.Trim(),

                Description = request.Description?.Trim(),

                StartDate = request.StartDate,

                DueDate = request.DueDate,

                CreatedAt = DateTime.UtcNow
            };

            _context.Milestones.Add(milestone);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                actorUserId,
                projectId,
                null,
                "MilestoneCreated",
                $"Milestone '{milestone.Name}' was created in project '{project.Name}'.");

            // -----------------------------------------------------
            // Notify project members about new milestone
            // -----------------------------------------------------

            await NotifyProjectMembersAboutMilestoneCreationAsync(
                projectId,
                milestone);

            return MapToResponse(milestone);
        }

        // =========================================================
        // GET ALL
        // =========================================================

        public async Task<List<MilestoneResponseDto>> GetAllAsync(
            Guid tenantId,
            Guid projectId)
        {
            await ValidateProjectAsync(
                tenantId,
                projectId);

            return await _context.Milestones
                .AsNoTracking()
                .Where(x =>
                    x.ProjectId == projectId)
                .OrderBy(x => x.DueDate)
                .ThenBy(x => x.Name)
                .Select(x => new MilestoneResponseDto
                {
                    Id = x.Id,

                    ProjectId = x.ProjectId,

                    Name = x.Name,

                    Description = x.Description,

                    StartDate = x.StartDate,

                    DueDate = x.DueDate,

                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<MilestoneResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId,
            Guid milestoneId)
        {
            await ValidateProjectAsync(
                tenantId,
                projectId);

            var milestone = await _context.Milestones
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == milestoneId &&
                    x.ProjectId == projectId);

            if (milestone is null)
            {
                throw new KeyNotFoundException(
                    "Milestone not found.");
            }

            return MapToResponse(milestone);
        }

        // =========================================================
        // UPDATE
        // =========================================================

        public async Task<MilestoneResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid milestoneId,
            Guid actorUserId,
            UpdateMilestoneRequestDto request)
        {
            var project = await ValidateProjectAsync(
                tenantId,
                projectId);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Milestone name is required.");
            }

            if (request.StartDate > request.DueDate)
            {
                throw new ArgumentException(
                    "Start date cannot be later than due date.");
            }

            var milestone = await _context.Milestones
                .FirstOrDefaultAsync(x =>
                    x.Id == milestoneId &&
                    x.ProjectId == projectId);

            if (milestone is null)
            {
                throw new KeyNotFoundException(
                    "Milestone not found.");
            }

            // -----------------------------------------------------
            // Store old values
            // -----------------------------------------------------

            var oldName = milestone.Name;
            var oldDescription = milestone.Description;
            var oldStartDate = milestone.StartDate;
            var oldDueDate = milestone.DueDate;

            // -----------------------------------------------------
            // Update milestone
            // -----------------------------------------------------

            milestone.Name =
                request.Name.Trim();

            milestone.Description =
                request.Description?.Trim();

            milestone.StartDate =
                request.StartDate;

            milestone.DueDate =
                request.DueDate;

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            var changes = new List<string>();

            if (oldName != milestone.Name)
            {
                changes.Add(
                    $"name changed from '{oldName}' to '{milestone.Name}'");
            }

            if (oldDescription != milestone.Description)
            {
                changes.Add("description updated");
            }

            if (oldStartDate != milestone.StartDate)
            {
                changes.Add(
                    $"start date changed from " +
                    $"{oldStartDate:dd MMM yyyy} to " +
                    $"{milestone.StartDate:dd MMM yyyy}");
            }

            if (oldDueDate != milestone.DueDate)
            {
                changes.Add(
                    $"due date changed from " +
                    $"{oldDueDate:dd MMM yyyy} to " +
                    $"{milestone.DueDate:dd MMM yyyy}");
            }

            if (changes.Count > 0)
            {
                await _activityLogService.CreateAsync(
                    tenantId,
                    actorUserId,
                    projectId,
                    null,
                    "MilestoneUpdated",
                    $"Milestone '{milestone.Name}' was updated in project '{project.Name}'. " +
                    $"Changes: {string.Join("; ", changes)}.");
            }

            // -----------------------------------------------------
            // Notify when start date changes
            // -----------------------------------------------------

            if (oldStartDate != milestone.StartDate)
            {
                await NotifyProjectMembersAboutStartDateChangeAsync(
                    projectId,
                    milestone);
            }

            // -----------------------------------------------------
            // Notify when due date changes
            // -----------------------------------------------------

            if (oldDueDate != milestone.DueDate)
            {
                await NotifyProjectMembersAboutDueDateChangeAsync(
                    projectId,
                    milestone);
            }

            return MapToResponse(milestone);
        }

        // =========================================================
        // DELETE
        // =========================================================

        public async System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid milestoneId,
            Guid actorUserId)
        {
            var project = await ValidateProjectAsync(
                tenantId,
                projectId);

            var milestone = await _context.Milestones
                .FirstOrDefaultAsync(x =>
                    x.Id == milestoneId &&
                    x.ProjectId == projectId);

            if (milestone is null)
            {
                throw new KeyNotFoundException(
                    "Milestone not found.");
            }

            // -----------------------------------------------------
            // Store information before deletion
            // -----------------------------------------------------

            var milestoneName = milestone.Name;

            // -----------------------------------------------------
            // Delete milestone
            // -----------------------------------------------------

            _context.Milestones.Remove(milestone);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                actorUserId,
                projectId,
                null,
                "MilestoneDeleted",
                $"Milestone '{milestoneName}' was deleted from project '{project.Name}'.");
        }

        // =========================================================
        // NOTIFY PROJECT MEMBERS - MILESTONE CREATED
        // =========================================================

        private async System.Threading.Tasks.Task
            NotifyProjectMembersAboutMilestoneCreationAsync(
                Guid projectId,
                Milestone milestone)
        {
            var projectMemberIds = await _context.ProjectMembers
                .AsNoTracking()
                .Where(x =>
                    x.ProjectId == projectId)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in projectMemberIds)
            {
                await _notificationService.CreateAsync(
                    userId,
                    new CreateNotificationRequestDto
                    {
                        Title = "New Milestone Created",

                        Message =
                            $"A new milestone '{milestone.Name}' " +
                            $"has been created in your project."
                    });
            }
        }

        // =========================================================
        // NOTIFY PROJECT MEMBERS - START DATE CHANGED
        // =========================================================

        private async System.Threading.Tasks.Task
            NotifyProjectMembersAboutStartDateChangeAsync(
                Guid projectId,
                Milestone milestone)
        {
            var projectMemberIds = await _context.ProjectMembers
                .AsNoTracking()
                .Where(x =>
                    x.ProjectId == projectId)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in projectMemberIds)
            {
                await _notificationService.CreateAsync(
                    userId,
                    new CreateNotificationRequestDto
                    {
                        Title = "Milestone Start Date Changed",

                        Message =
                            $"The start date for milestone " +
                            $"'{milestone.Name}' has been changed to " +
                            $"{milestone.StartDate:dd MMM yyyy}."
                    });
            }
        }

        // =========================================================
        // NOTIFY PROJECT MEMBERS - DUE DATE CHANGED
        // =========================================================

        private async System.Threading.Tasks.Task
            NotifyProjectMembersAboutDueDateChangeAsync(
                Guid projectId,
                Milestone milestone)
        {
            var projectMemberIds = await _context.ProjectMembers
                .AsNoTracking()
                .Where(x =>
                    x.ProjectId == projectId)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in projectMemberIds)
            {
                await _notificationService.CreateAsync(
                    userId,
                    new CreateNotificationRequestDto
                    {
                        Title = "Milestone Due Date Changed",

                        Message =
                            $"The due date for milestone " +
                            $"'{milestone.Name}' has been changed to " +
                            $"{milestone.DueDate:dd MMM yyyy}."
                    });
            }
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
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // -----------------------------------------------------
            // Check project
            // -----------------------------------------------------

            var project = await _context.Projects
                .FirstOrDefaultAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (project is null)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            return project;
        }

        // =========================================================
        // MAP RESPONSE
        // =========================================================

        private static MilestoneResponseDto MapToResponse(
            Milestone milestone)
        {
            return new MilestoneResponseDto
            {
                Id = milestone.Id,

                ProjectId = milestone.ProjectId,

                Name = milestone.Name,

                Description = milestone.Description,

                StartDate = milestone.StartDate,

                DueDate = milestone.DueDate,

                CreatedAt = milestone.CreatedAt
            };
        }
    }
}