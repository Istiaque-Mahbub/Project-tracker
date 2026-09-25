using DeveloperProjectTrackerDomain.Entities;
using DeveloperProjectTrackerDomain.Enums;

namespace DeveloperProjectTracker.API.Services.Auth
{
    public interface IJwtService
    {
        string GenerateAccessToken(
          User user,
          Guid? tenantId = null,
          TenantRole? tenantRole = null);

        string GenerateRefreshToken();
    }
}
