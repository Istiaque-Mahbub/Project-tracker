using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Milestones;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Milestones
{
    public class MilestoneService : IMilestoneService
    {
        private readonly ApplicationDbContext _context;

        public MilestoneService(
            ApplicationDbContext context)
        {
            _context = context;
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

            milestone.Name = request.Name.Trim();

            milestone.Description =
                request.Description?.Trim();

            milestone.StartDate =
                request.StartDate;

            milestone.DueDate =
                request.DueDate;

            await _context.SaveChangesAsync();

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
        // VALIDATE PROJECT
        // =========================================================

        private async System.Threading.Tasks.Task ValidateProjectAsync(
            Guid tenantId,
            Guid projectId)
        {
            var tenantExists = await _context.Tenants
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

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