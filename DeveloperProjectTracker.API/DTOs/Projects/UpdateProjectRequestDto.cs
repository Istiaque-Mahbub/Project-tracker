using System.ComponentModel.DataAnnotations;

namespace DeveloperProjectTracker.API.DTOs.Projects
{
    public class UpdateProjectRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description
        {
            get; set;
        }
    }
}
