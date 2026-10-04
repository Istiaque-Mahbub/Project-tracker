namespace DeveloperProjectTracker.API.DTOs.Auth
{
    public class SelectTenantResponseDto
    {
        public Guid TenantId { get; set; }

        public string TenantName { get; set; } = string.Empty;

        public string TenantRole { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public DateTime AccessTokenExpiresAt { get; set; }
    }
}
