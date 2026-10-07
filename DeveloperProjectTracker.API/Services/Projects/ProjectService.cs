using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Projects;
using DeveloperProjectTracker.API.Services.ActivityLogs;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IActivityLogService _activityLogService;

        public ProjectService(
            ApplicationDbContext context,
            IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;
        }

        // =========================================================
        // CREATE PROJECT
        // =========================================================

        public async Task<ProjectResponseDto> CreateAsync(
            Guid tenantId,
            Guid actorUserId,
            CreateProjectRequestDto request)
        {
            // -----------------------------------------------------
            // Validate tenant
            // -----------------------------------------------------

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (tenant is null)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            // -----------------------------------------------------
            // Validate project name
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Project name is required.");
            }

            // -----------------------------------------------------
            // Validate actor
            // -----------------------------------------------------

            var owner = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == actorUserId);

            if (owner is null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            // -----------------------------------------------------
            // Create project
            // -----------------------------------------------------

            var project = new Project
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                OwnerId = actorUserId,
                CreateAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                actorUserId,
                project.Id,
                null,
                "ProjectCreated",
                $"User '{owner.FullName}' created project '{project.Name}'.");

            return MapToResponse(project);
        }

        // =========================================================
        // GET ALL PROJECTS
        // =========================================================

        public async Task<List<ProjectResponseDto>> GetAllAsync(
            Guid tenantId)
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

            return await _context.Projects
                .AsNoTracking()
                .Where(p => p.TenantId == tenantId)
                .OrderBy(p => p.Name)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    TenantId = p.TenantId,
                    Name = p.Name,
                    Description = p.Description,
                    OwnerId = p.OwnerId,
                    CreateAt = p.CreateAt,
                    UpdateAt = p.UpdateAt
                })
                .ToListAsync();
        }

        // =========================================================
        // GET SINGLE PROJECT
        // =========================================================

        public async Task<ProjectResponseDto> GetByIdAsync(
            Guid tenantId,
            Guid projectId)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (project is null)
            {
                throw new KeyNotFoundException(
                    "Project not found.");
            }

            return MapToResponse(project);
        }

        // =========================================================
        // UPDATE PROJECT
        // =========================================================

        public async Task<ProjectResponseDto> UpdateAsync(
            Guid tenantId,
            Guid projectId,
            Guid actorUserId,
            UpdateProjectRequestDto request)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (project is null)
            {
                throw new KeyNotFoundException(
                    "Project not found.");
            }

            // -----------------------------------------------------
            // Validate project name
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Project name is required.");
            }

            // -----------------------------------------------------
            // Validate actor
            // -----------------------------------------------------

            var actor = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == actorUserId);

            if (actor is null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            // -----------------------------------------------------
            // Store old values
            // -----------------------------------------------------

            var oldName = project.Name;
            var oldDescription = project.Description;

            // -----------------------------------------------------
            // Update project
            // -----------------------------------------------------

            project.Name = request.Name.Trim();
            project.Description =
                request.Description?.Trim();

            project.UpdateAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Build changes
            // -----------------------------------------------------

            var changes = new List<string>();

            if (!string.Equals(
                    oldName,
                    project.Name,
                    StringComparison.Ordinal))
            {
                changes.Add(
                    $"Name changed from '{oldName}' to '{project.Name}'");
            }

            if (!string.Equals(
                    oldDescription,
                    project.Description,
                    StringComparison.Ordinal))
            {
                changes.Add(
                    "Description changed");
            }

            // -----------------------------------------------------
            // Activity Log
            // -----------------------------------------------------

            if (changes.Count > 0)
            {
                await _activityLogService.CreateAsync(
                    tenantId,
                    actorUserId,
                    project.Id,
                    null,
                    "ProjectUpdated",
                    $"User '{actor.FullName}' updated project '{project.Name}'. Changes: {string.Join(", ", changes)}.");
            }

            return MapToResponse(project);
        }

        // =========================================================
        // DELETE PROJECT
        // =========================================================

        public async System.Threading.Tasks.Task DeleteAsync(
            Guid tenantId,
            Guid projectId,
            Guid actorUserId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (project is null)
            {
                throw new KeyNotFoundException(
                    "Project not found.");
            }

            // -----------------------------------------------------
            // Validate actor
            // -----------------------------------------------------

            var actor = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == actorUserId);

            if (actor is null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            // -----------------------------------------------------
            // Activity Log BEFORE deletion
            // -----------------------------------------------------

            await _activityLogService.CreateAsync(
                tenantId,
                actorUserId,
                project.Id,
                null,
                "ProjectDeleted",
                $"User '{actor.FullName}' deleted project '{project.Name}'.");

            // -----------------------------------------------------
            // Delete project
            // -----------------------------------------------------

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // MAP RESPONSE
        // =========================================================

        private static ProjectResponseDto MapToResponse(
            Project project)
        {
            return new ProjectResponseDto
            {
                Id = project.Id,
                TenantId = project.TenantId,
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                CreateAt = project.CreateAt,
                UpdateAt = project.UpdateAt
            };
        }
    }
}