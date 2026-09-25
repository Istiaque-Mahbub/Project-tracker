namespace DeveloperProjectTracker.API.DTOs.Tenant
{
    public class UpdateTenantRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;
    }
}
