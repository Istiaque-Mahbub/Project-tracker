using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Projects;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DeveloperProjectTracker.API.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProjectResponseDto> CreateAsync(
            Guid tenantId,
            CreateProjectRequestDto request)
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t =>
                    t.Id == tenantId &&
                    t.IsActive);

            if (tenant is null)
            {
                throw new KeyNotFoundException(
                    "Tenant not found or inactive.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Project name is required.");
            }

            // Get logged-in user from JWT
            var userIdValue = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var ownerId))
            {
                throw new UnauthorizedAccessException(
                    "Unable to identify the current user.");
            }

            // Make sure the user actually exists
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == ownerId);

            if (!userExists)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                OwnerId = ownerId,
                CreateAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            return MapToResponse(project);
        }

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

        public async Task<ProjectResponseDto> UpdateAsync(
                     Guid tenantId,
                     Guid projectId,
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

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Project name is required.");
            }

            project.Name = request.Name.Trim();
            project.Description = request.Description?.Trim();
            project.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(project);
        }

        public async System.Threading.Tasks.Task DeleteAsync(
    Guid tenantId,
    Guid projectId)
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

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();
        }

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