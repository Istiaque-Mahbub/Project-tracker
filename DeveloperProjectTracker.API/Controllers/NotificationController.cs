using System.Security.Claims;
using DeveloperProjectTracker.API.DTOs.Notifications;
using DeveloperProjectTracker.API.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // =========================================================
        // GET MY NOTIFICATIONS
        // =========================================================

        [HttpGet]
        public async Task<
            ActionResult<List<NotificationResponseDto>>>
            GetMyNotifications()
        {
            try
            {
                var userId = GetCurrentUserId();

                var notifications =
                    await _notificationService
                        .GetMyNotificationsAsync(userId);

                return Ok(notifications);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
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
                        message =
                            "An unexpected error occurred."
                    });
            }
        }

        // =========================================================
        // GET SINGLE NOTIFICATION
        // =========================================================

        [HttpGet("{notificationId:guid}")]
        public async Task<
            ActionResult<NotificationResponseDto>>
            GetById(Guid notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var notification =
                    await _notificationService.GetByIdAsync(
                        userId,
                        notificationId);

                return Ok(notification);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
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
                        message =
                            "An unexpected error occurred."
                    });
            }
        }

        // =========================================================
        // CREATE NOTIFICATION
        // =========================================================

        [HttpPost]
        public async Task<
            ActionResult<NotificationResponseDto>>
            Create(CreateNotificationRequestDto request)
        {
            try
            {
                var notification =
                    await _notificationService.CreateAsync(
                        request.UserId,
                        request);

                return Ok(notification);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
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
                        message =
                            "An unexpected error occurred."
                    });
            }
        }

        // =========================================================
        // MARK AS READ
        // =========================================================

        [HttpPatch("{notificationId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(
            Guid notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _notificationService.MarkAsReadAsync(
                    userId,
                    notificationId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
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
                        message =
                            "An unexpected error occurred."
                    });
            }
        }

        // =========================================================
        // MARK ALL AS READ
        // =========================================================

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();

                await _notificationService.MarkAllAsReadAsync(
                    userId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
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
                        message =
                            "An unexpected error occurred."
                    });
            }
        }

        // =========================================================
        // DELETE
        // =========================================================

        [HttpDelete("{notificationId:guid}")]
        public async Task<IActionResult> Delete(
            Guid notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _notificationService.DeleteAsync(
                    userId,
                    notificationId);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
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
                        message =
                            "An unexpected error occurred."
                    });
            }
        }

        // =========================================================
        // CURRENT USER ID
        // =========================================================

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException(
                    "Unable to identify the current user.");
            }

            return userId;
        }
    }
}