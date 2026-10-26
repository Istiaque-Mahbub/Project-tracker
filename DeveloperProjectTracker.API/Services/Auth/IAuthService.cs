using DeveloperProjectTracker.API.DTOs.Auth;

namespace DeveloperProjectTracker.API.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(
            RegisterRequestDto request);

        Task<AuthResponseDto> LoginAsync(
            LoginRequestDto request);

        Task<AuthResponseDto> RefreshTokenAsync(
            RefreshTokenRequestDto request);

        Task<SelectTenantResponseDto> SelectTenantAsync(
            Guid userId,
            Guid tenantId);

        System.Threading.Tasks.Task LogoutAsync(
            RefreshTokenRequestDto request);
    }
}
