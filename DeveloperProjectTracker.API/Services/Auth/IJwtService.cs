using DeveloperProjectTrackerDomain.Entities;

namespace DeveloperProjectTracker.API.Services.Auth
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();
    }
}
