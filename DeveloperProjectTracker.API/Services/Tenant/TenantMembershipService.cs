using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Tenant;
using DeveloperProjectTrackerDomain.Entities;
using DeveloperProjectTrackerDomain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Tenant
{
    public class TenantMembershipService : ITenantMembershipService
    {
        private readonly ApplicationDbContext _context;

        public TenantMembershipService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TenantMemberResponseDto> AddMemberAsync(
           Guid tenantId,
           AddTenantMemberRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            // Check tenant
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

            // Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                throw new KeyNotFoundException(
                    "User with this email does not exist.");
            }

            // Check existing membership
            var existingMembership =
                await _context.TenantMemberships
                    .FirstOrDefaultAsync(x =>
                        x.TenantId == tenantId &&
                        x.UserId == user.Id);

            if (existingMembership is not null)
            {
                throw new InvalidOperationException(
                    "User is already a member of this tenant.");
            }

            // Prevent assigning invalid tenant role
            if (!Enum.IsDefined(typeof(TenantRole), request.Role))
            {
                throw new ArgumentException(
                    "Invalid tenant role.");
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

            return new TenantMemberResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = membership.Role,
                JoinedAt = membership.JoinedAt
            };
        }

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

        public async Task<TenantMemberResponseDto> GetMemberAsync(
            Guid tenantId,
            Guid userId)
        {
            var membership =
                await _context.TenantMemberships
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x =>
                        x.TenantId == tenantId &&
                        x.UserId == userId);

            if (membership is null)
            {
                throw new KeyNotFoundException(
                    "User is not a member of this tenant.");
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

        public async Task<TenantMemberResponseDto> UpdateRoleAsync(
            Guid tenantId,
            Guid userId,
            UpdateTenantMemberRoleRequestDto request)
        {
            if (!Enum.IsDefined(
                typeof(TenantRole),
                request.Role))
            {
                throw new ArgumentException(
                    "Invalid tenant role.");
            }

            var membership =
                await _context.TenantMemberships
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x =>
                        x.TenantId == tenantId &&
                        x.UserId == userId);

            if (membership is null)
            {
                throw new KeyNotFoundException(
                    "User is not a member of this tenant.");
            }

            membership.Role = request.Role;

            await _context.SaveChangesAsync();

            return new TenantMemberResponseDto
            {
                UserId = membership.UserId,
                FullName = membership.User.FullName,
                Email = membership.User.Email,
                Role = membership.Role,
                JoinedAt = membership.JoinedAt
            };
        }

        public async Task RemoveMemberAsync(
            Guid tenantId,
            Guid userId)
        {
            var membership =
                await _context.TenantMemberships
                    .FirstOrDefaultAsync(x =>
                        x.TenantId == tenantId &&
                        x.UserId == userId);

            if (membership is null)
            {
                throw new KeyNotFoundException(
                    "User is not a member of this tenant.");
            }

            _context.TenantMemberships.Remove(membership);

            await _context.SaveChangesAsync();
        }
    }
}
