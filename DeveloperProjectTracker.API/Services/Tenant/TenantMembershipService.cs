using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Notifications;
using DeveloperProjectTracker.API.DTOs.Tenant;
using DeveloperProjectTracker.API.Services.Notifications;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Tenant
{
    public class TenantMembershipService : ITenantMembershipService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public TenantMembershipService(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // =========================================================
        // ADD MEMBER
        // =========================================================

        public async Task<TenantMemberResponseDto> AddMemberAsync(
            Guid tenantId,
            AddTenantMemberRequestDto request)
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant is null)
            {
                throw new KeyNotFoundException(
                    "Tenant not found.");
            }

            if (!tenant.IsActive)
            {
                throw new InvalidOperationException(
                    "Tenant is inactive.");
            }

            var email = request.Email
                .Trim()
                .ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                throw new KeyNotFoundException(
                    "User not found.");
            }

            var alreadyMember = await _context.TenantMemberships
                .AnyAsync(x =>
                    x.TenantId == tenantId &&
                    x.UserId == user.Id);

            if (alreadyMember)
            {
                throw new InvalidOperationException(
                    "User is already a member of this tenant.");
            }

            var membership = new TenantMembership
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = user.Id,
                Role = request.Role,
                JoinedAt = DateTime.UtcNow
            };

            _context.TenantMemberships.Add(membership);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Send notification to the newly added user
            // -----------------------------------------------------

            await _notificationService.CreateAsync(
                user.Id,
                new CreateNotificationRequestDto
                {
                    Title = "Added to Tenant",
                    Message =
                        $"You have been added to tenant '{tenant.Name}' " +
                        $"with the role '{membership.Role}'."
                });

            return new TenantMemberResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = membership.Role,
                JoinedAt = membership.JoinedAt
            };
        }

        // =========================================================
        // GET ALL MEMBERS
        // =========================================================

        public async Task<List<TenantMemberResponseDto>> GetMembersAsync(
            Guid tenantId)
        {
            var tenantExists = await _context.Tenants
                .AnyAsync(t => t.Id == tenantId);

            if (!tenantExists)
            {
                throw new KeyNotFoundException(
                    "Tenant not found.");
            }

            return await _context.TenantMemberships
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .Include(x => x.User)
                .OrderBy(x => x.User.FullName)
                .Select(x => new TenantMemberResponseDto
                {
                    UserId = x.UserId,
                    FullName = x.User.FullName,
                    Email = x.User.Email,
                    Role = x.Role,
                    JoinedAt = x.JoinedAt
                })
                .ToListAsync();
        }

        // =========================================================
        // GET SINGLE MEMBER
        // =========================================================

        public async Task<TenantMemberResponseDto> GetMemberAsync(
            Guid tenantId,
            Guid userId)
        {
            var membership = await _context.TenantMemberships
                .AsNoTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.TenantId == tenantId &&
                    x.UserId == userId);

            if (membership is null)
            {
                throw new KeyNotFoundException(
                    "Tenant member not found.");
            }

            return new TenantMemberResponseDto
            {
                UserId = membership.UserId,
                FullName = membership.User.FullName,
                Email = membership.User.Email,
                Role = membership.Role,
                JoinedAt = membership.JoinedAt
            };
        }

        // =========================================================
        // UPDATE MEMBER ROLE
        // =========================================================

        public async Task<TenantMemberResponseDto> UpdateRoleAsync(
            Guid tenantId,
            Guid userId,
            UpdateTenantMemberRoleRequestDto request)
        {
            var membership = await _context.TenantMemberships
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.TenantId == tenantId &&
                    x.UserId == userId);

            if (membership is null)
            {
                throw new KeyNotFoundException(
                    "Tenant member not found.");
            }

            var oldRole = membership.Role;

            // Nothing changed
            if (oldRole == request.Role)
            {
                return new TenantMemberResponseDto
                {
                    UserId = membership.UserId,
                    FullName = membership.User.FullName,
                    Email = membership.User.Email,
                    Role = membership.Role,
                    JoinedAt = membership.JoinedAt
                };
            }

            membership.Role = request.Role;

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Notify user about role change
            // -----------------------------------------------------

            await _notificationService.CreateAsync(
                membership.UserId,
                new CreateNotificationRequestDto
                {
                    Title = "Tenant Role Changed",
                    Message =
                        $"Your role has been changed from " +
                        $"'{oldRole}' to '{membership.Role}'."
                });

            return new TenantMemberResponseDto
            {
                UserId = membership.UserId,
                FullName = membership.User.FullName,
                Email = membership.User.Email,
                Role = membership.Role,
                JoinedAt = membership.JoinedAt
            };
        }

        // =========================================================
        // REMOVE MEMBER
        // =========================================================

        public async System.Threading.Tasks.Task RemoveMemberAsync(
            Guid tenantId,
            Guid userId)
        {
            var membership = await _context.TenantMemberships
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.TenantId == tenantId &&
                    x.UserId == userId);

            if (membership is null)
            {
                throw new KeyNotFoundException(
                    "Tenant member not found.");
            }

            var userIdToNotify = membership.UserId;

            var userName = membership.User.FullName;

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(x =>
                    x.Id == tenantId);

            if (tenant is null)
            {
                throw new KeyNotFoundException(
                    "Tenant not found.");
            }

            _context.TenantMemberships.Remove(membership);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Notify removed user
            // -----------------------------------------------------

            await _notificationService.CreateAsync(
                userIdToNotify,
                new CreateNotificationRequestDto
                {
                    Title = "Removed from Tenant",
                    Message =
                        $"You have been removed from tenant " +
                        $"'{tenant.Name}'."
                });
        }
    }
}