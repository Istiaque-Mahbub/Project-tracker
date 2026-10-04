using DeveloperProjectTracker.API.Data;
using DeveloperProjectTrackerDomain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DeveloperProjectTracker.API.Authorization
{
    public class TenantAdminHandler : AuthorizationHandler<TenantAdminRequirement>
    {
        private readonly ApplicationDbContext _context;

        public TenantAdminHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           TenantAdminRequirement requirement)
        {
            // =====================================================
            // 1. Check if user is SuperAdmin
            // =====================================================

            var globalRole = context.User.FindFirst(
                ClaimTypes.Role)?.Value;

            if (globalRole == UserRole.SuperAdmin.ToString())
            {
                context.Succeed(requirement);
                return;
            }

            // =====================================================
            // 2. Get current User ID
            // =====================================================

            var userIdClaim = context.User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return;
            }

            // =====================================================
            // 3. Get tenantId from route
            // =====================================================

            if (context.Resource is not HttpContext httpContext)
            {
                return;
            }

            var tenantIdValue =
                httpContext.Request.RouteValues["tenantId"]?.ToString();

            if (!Guid.TryParse(tenantIdValue, out var tenantId))
            {
                return;
            }

            // =====================================================
            // 4. Check TenantMembership
            // =====================================================

            var isTenantAdmin =
                await _context.TenantMemberships
                    .AnyAsync(x =>
                        x.TenantId == tenantId &&
                        x.UserId == userId &&
                        x.Role == TenantRole.Admin);

            if (isTenantAdmin)
            {
                context.Succeed(requirement);
            }
        }
    }
}
