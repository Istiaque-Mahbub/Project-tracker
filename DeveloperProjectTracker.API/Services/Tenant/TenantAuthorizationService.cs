using DeveloperProjectTracker.API.Data;
using DeveloperProjectTrackerDomain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Tenant
{
    public class TenantAuthorizationService : ITenantAuthorizationService
    {
        private readonly ApplicationDbContext _context;

        public TenantAuthorizationService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsMemberAsync(
            Guid userId,
            Guid tenantId)
        {
            return await _context.TenantMemberships
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.TenantId == tenantId);
        }

        public async Task<bool> HasRoleAsync(
            Guid userId,
            Guid tenantId,
            params TenantRole[] roles)
        {
            return await _context.TenantMemberships
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.TenantId == tenantId &&
                    roles.Contains(x.Role));
        }

        public async Task<bool> IsTenantAdminAsync(
            Guid userId,
            Guid tenantId)
        {
            return await HasRoleAsync(
                userId,
                tenantId,
                TenantRole.Admin);
        }
    }
}
