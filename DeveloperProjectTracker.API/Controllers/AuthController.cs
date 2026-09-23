using DeveloperProjectTracker.API.DTOs.Auth;
using DeveloperProjectTracker.API.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperProjectTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]

        public async Task<ActionResult<AuthResponseDto>> Register(
            RegisterRequestDto request)

        {

            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }

        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(
      LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
        }


        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh(
            RefreshTokenRequestDto request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request);

                return Ok(result);
            }
            catch (NotImplementedException ex)
            {
                return StatusCode(StatusCodes.Status501NotImplemented, new
                {
                    message = ex.Message
                });
            }

        }
    }
}
