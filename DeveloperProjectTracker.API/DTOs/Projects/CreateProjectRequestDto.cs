using System.ComponentModel.DataAnnotations;

namespace DeveloperProjectTracker.API.DTOs.Projects
{
    public class CreateProjectRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
