using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.ProjectMembers;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.ProjectMembers
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly ApplicationDbContext _context;

        public ProjectMemberService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ADD MEMBER
        // =========================================================

        public async Task<ProjectMemberResponseDto> AddMemberAsync(
            Guid tenantId,
            Guid projectId,
            AddProjectMemberRequestDto request)
        {
            // -----------------------------------------------------
            // 1. Check tenant
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
            // 2. Check project belongs to tenant
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
            // 3. Check user exists
            // -----------------------------------------------------

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == request.UserId);

            if (user is null)
            {
                throw new KeyNotFoundException(
                    "User not found.");
            }

            // -----------------------------------------------------
            // 4. Check user belongs to tenant
            // -----------------------------------------------------

            var tenantMember = await _context.TenantMemberships
                .AnyAsync(x =>
                    x.TenantId == tenantId &&
                    x.UserId == request.UserId);

            if (!tenantMember)
            {
                throw new ArgumentException(
                    "User is not a member of this tenant.");
            }

            // -----------------------------------------------------
            // 5. Check existing project membership
            // -----------------------------------------------------

            var existingMember = await _context.ProjectMembers
                .AnyAsync(x =>
                    x.ProjectId == projectId &&
                    x.UserId == request.UserId);

            if (existingMember)
            {
                throw new InvalidOperationException(
                    "User is already a member of this project.");
            }

            // -----------------------------------------------------
            // 6. Create project membership
            // -----------------------------------------------------

            var projectMember = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = request.UserId
            };

            _context.ProjectMembers.Add(projectMember);

            await _context.SaveChangesAsync();

            return new ProjectMemberResponseDto
            {
                Id = projectMember.Id,
                ProjectId = projectMember.ProjectId,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };
        }

        // =========================================================
        // GET ALL MEMBERS
        // =========================================================

        public async Task<List<ProjectMemberResponseDto>> GetMembersAsync(
            Guid tenantId,
            Guid projectId)
        {
            var projectExists = await _context.Projects
                .AnyAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            return await _context.ProjectMembers
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .Include(x => x.User)
                .Select(x => new ProjectMemberResponseDto
                {
                    Id = x.Id,
                    ProjectId = x.ProjectId,
                    UserId = x.UserId,
                    FullName = x.User.FullName,
                    Email = x.User.Email
                })
                .ToListAsync();
        }

        // =========================================================
        // GET SINGLE MEMBER
        // =========================================================

        public async Task<ProjectMemberResponseDto> GetMemberAsync(
            Guid tenantId,
            Guid projectId,
            Guid userId)
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

            var member = await _context.ProjectMembers
                .AsNoTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.ProjectId == projectId &&
                    x.UserId == userId);

            if (member is null)
            {
                throw new KeyNotFoundException(
                    "Project member not found.");
            }

            return new ProjectMemberResponseDto
            {
                Id = member.Id,
                ProjectId = member.ProjectId,
                UserId = member.UserId,
                FullName = member.User.FullName,
                Email = member.User.Email
            };
        }

        // =========================================================
        // REMOVE MEMBER
        // =========================================================

        public async System.Threading.Tasks.Task RemoveMemberAsync(
            Guid tenantId,
            Guid projectId,
            Guid userId)
        {
            var projectExists = await _context.Projects
                .AnyAsync(p =>
                    p.Id == projectId &&
                    p.TenantId == tenantId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "Project not found in this tenant.");
            }

            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(x =>
                    x.ProjectId == projectId &&
                    x.UserId == userId);

            if (member is null)
            {
                throw new KeyNotFoundException(
                    "Project member not found.");
            }

            _context.ProjectMembers.Remove(member);

            await _context.SaveChangesAsync();
        }


    }
}
