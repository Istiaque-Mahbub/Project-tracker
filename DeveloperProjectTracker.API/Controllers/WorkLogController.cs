using System.Security.Claims;
using DeveloperProjectTracker.API.DTOs.WorkLogs;
using DeveloperProjectTracker.API.Services.WorkLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route(
        "api/tenants/{tenantId:guid}/projects/{projectId:guid}/tasks/{taskId:guid}/worklogs")]
    [Authorize]
    public class WorkLogController : ControllerBase
    {
        private readonly IWorkLogService _workLogService;

        public WorkLogController(
            IWorkLogService workLogService)
        {
            _workLogService = workLogService;
        }

        // =========================================================
        // CREATE WORK LOG
        // =========================================================

        [HttpPost]
        public async Task<ActionResult<WorkLogResponseDto>> Create(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            CreateWorkLogRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var workLog =
                    await _workLogService.CreateAsync(
                        tenantId,
                        projectId,
                        taskId,
                        userId,
                        request);

                return Ok(workLog);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
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
        // GET ALL WORK LOGS
        // =========================================================

        [HttpGet]
        public async Task<
            ActionResult<List<WorkLogResponseDto>>> GetAll(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            try
            {
                var workLogs =
                    await _workLogService.GetAllAsync(
                        tenantId,
                        projectId,
                        taskId);

                return Ok(workLogs);
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
        // GET SINGLE WORK LOG
        // =========================================================

        [HttpGet("{workLogId:guid}")]
        public async Task<ActionResult<WorkLogResponseDto>>
            GetById(
                Guid tenantId,
                Guid projectId,
                Guid taskId,
                Guid workLogId)
        {
            try
            {
                var workLog =
                    await _workLogService.GetByIdAsync(
                        tenantId,
                        projectId,
                        taskId,
                        workLogId);

                return Ok(workLog);
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
        // UPDATE WORK LOG
        // =========================================================

        [HttpPut("{workLogId:guid}")]
        public async Task<ActionResult<WorkLogResponseDto>>
            Update(
                Guid tenantId,
                Guid projectId,
                Guid taskId,
                Guid workLogId,
                UpdateWorkLogRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var workLog =
                    await _workLogService.UpdateAsync(
                        tenantId,
                        projectId,
                        taskId,
                        workLogId,
                        userId,
                        request);

                return Ok(workLog);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
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
        // DELETE WORK LOG
        // =========================================================

        [HttpDelete("{workLogId:guid}")]
        public async Task<IActionResult> Delete(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid workLogId)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _workLogService.DeleteAsync(
                    tenantId,
                    projectId,
                    taskId,
                    workLogId,
                    userId);

                return NoContent();
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
        // CURRENT USER
        // =========================================================

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(
                    userIdValue,
                    out var userId))
            {
                throw new UnauthorizedAccessException(
                    "Unable to identify the current user.");
            }

            return userId;
        }
    }
}