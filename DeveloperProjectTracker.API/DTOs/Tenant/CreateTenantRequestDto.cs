namespace DeveloperProjectTracker.API.DTOs.Tenant
{
    public class CreateTenantRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;
    }
}
