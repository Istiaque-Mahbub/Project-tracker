using DeveloperProjectTracker.API.DTOs.Auth;
using DeveloperProjectTracker.API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpPost("select-tenant")]
        [Authorize]
        public async Task<ActionResult<AuthResponseDto>> SelectTenant(
    SelectTenantRequestDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new
                    {
                        message = "Unable to identify the current user."
                    });
                }

                var response = await _authService.SelectTenantAsync(
                    userId,
                    request.TenantId);

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "An unexpected error occurred."
                    });
            }
        }
    }
}
