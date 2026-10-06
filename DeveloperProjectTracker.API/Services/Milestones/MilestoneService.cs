using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Milestones;
using DeveloperProjectTracker.API.DTOs.Notifications;
using DeveloperProjectTracker.API.Services.Notifications;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Milestones
{
    public class MilestoneService : IMilestoneService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public MilestoneService(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // =========================================================
        // CREATE
        // =========================================================

        public async Task<MilestoneResponseDto> CreateAsync(
            Guid tenantId,
            Guid projectId,
            CreateMilestoneRequestDto request)
        {
            await ValidateProjectAsync(
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
            UpdateMilestoneRequestDto request)
        {
            await ValidateProjectAsync(
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
            // Store old values before updating
            // -----------------------------------------------------

            var oldDueDate = milestone.DueDate;

            var oldName = milestone.Name;

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
            // Notify project members when due date changes
            // -----------------------------------------------------

            if (oldDueDate != milestone.DueDate)
            {
                await NotifyProjectMembersAboutDueDateChangeAsync(
                    tenantId,
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
            Guid milestoneId)
        {
            await ValidateProjectAsync(
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

            _context.Milestones.Remove(milestone);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // NOTIFY PROJECT MEMBERS
        // =========================================================

        private async System.Threading.Tasks.Task NotifyProjectMembersAboutDueDateChangeAsync(
            Guid tenantId,
            Guid projectId,
            Milestone milestone)
        {
            // -----------------------------------------------------
            // Get project members
            // -----------------------------------------------------

            var projectMemberIds = await _context.ProjectMembers
                .AsNoTracking()
                .Where(x =>
                    x.ProjectId == projectId)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();

            // -----------------------------------------------------
            // Also get tenant/project users if needed later.
            //
            // Currently we notify only ProjectMembers.
            // -----------------------------------------------------

            foreach (var userId in projectMemberIds)
            {
                await _notificationService.CreateAsync(
                    userId,
                    new CreateNotificationRequestDto
                    {
                        Title = "Milestone Due Date Changed",

                        Message =
                            $"The due date for milestone '{milestone.Name}' " +
                            $"has been changed to " +
                            $"{milestone.DueDate:dd MMM yyyy}."
                    });
            }
        }

        // =========================================================
        // VALIDATE PROJECT
        // =========================================================

        private async System.Threading.Tasks.Task ValidateProjectAsync(
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

            var projectExists = await _context.Projects
                .AnyAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }
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