using System.ComponentModel.DataAnnotations;

namespace DeveloperProjectTracker.API.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
