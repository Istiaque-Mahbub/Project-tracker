using System.ComponentModel.DataAnnotations;

namespace DeveloperProjectTracker.API.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
