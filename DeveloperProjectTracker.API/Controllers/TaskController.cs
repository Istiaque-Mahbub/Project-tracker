
using DeveloperProjectTracker.API.DTOs.Tasks;
using DeveloperProjectTracker.API.Services.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route("api/tenants/{tenantId:guid}/projects/{projectId:guid}/tasks")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TaskController> _logger;

        public TaskController(
            ITaskService taskService,
            ILogger<TaskController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        // =========================================================
        // CREATE TASK
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<TaskResponseDto>> Create(
            Guid tenantId,
            Guid projectId,
            CreateTaskRequestDto request)
        {
            try
            {
                var userIdValue =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdValue, out var userId))
                {
                    return Unauthorized(new
                    {
                        message = "Unable to identify the current user."
                    });
                }

                var task = await _taskService.CreateAsync(
                    tenantId,
                    projectId,
                    userId,
                    request);

                return CreatedAtAction(
                    nameof(GetById),
                    new
                    {
                        tenantId,
                        projectId,
                        taskId = task.Id
                    },
                    task);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create task for tenant {TenantId}, project {ProjectId}",
                    tenantId,
                    projectId);

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                });
            }
        }

        // =========================================================
        // GET ALL TASKS
        // =========================================================

        [HttpGet]
        public async Task<ActionResult<List<TaskResponseDto>>> GetAll(
            Guid tenantId,
            Guid projectId)
        {
            try
            {
                var tasks = await _taskService.GetAllAsync(
                    tenantId,
                    projectId);

                return Ok(tasks);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve tasks for tenant {TenantId}, project {ProjectId}",
                    tenantId,
                    projectId);

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                });
            }
        }

        // =========================================================
        // GET SINGLE TASK
        // =========================================================

        [HttpGet("{taskId:guid}")]
        public async Task<ActionResult<TaskResponseDto>> GetById(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            try
            {
                var task = await _taskService.GetByIdAsync(
                    tenantId,
                    projectId,
                    taskId);

                return Ok(task);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve task {TaskId} for tenant {TenantId}, project {ProjectId}",
                    taskId,
                    tenantId,
                    projectId);

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                });
            }
        }

        // =========================================================
        // UPDATE TASK
        // =========================================================

        [HttpPut("{taskId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<TaskResponseDto>> Update(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            UpdateTaskRequestDto request)
        {
            try
            {
                var userIdValue =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdValue, out var userId))
                {
                    return Unauthorized(new
                    {
                        message = "Unable to identify the current user."
                    });
                }

                var task = await _taskService.UpdateAsync(
                    tenantId,
                    projectId,
                    taskId,
                    userId,
                    request);

                return Ok(task);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update task {TaskId} for tenant {TenantId}, project {ProjectId}",
                    taskId,
                    tenantId,
                    projectId);

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                });
            }
        }

        // =========================================================
        // DELETE TASK
        // =========================================================

        [HttpDelete("{taskId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<IActionResult> Delete(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            try
            {
                var userIdValue =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdValue, out var userId))
                {
                    return Unauthorized(new
                    {
                        message = "Unable to identify the current user."
                    });
                }

                await _taskService.DeleteAsync(
                    tenantId,
                    projectId,
                    taskId,
                    userId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete task {TaskId} for tenant {TenantId}, project {ProjectId}",
                    taskId,
                    tenantId,
                    projectId);

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                });
            }
        }

        // =========================================================
        // ASSIGN TASK
        // =========================================================

        [HttpPut("{taskId:guid}/assign")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<TaskResponseDto>> Assign(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            AssignTaskRequestDto request)
        {
            try
            {
                var userIdValue =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdValue, out var userId))
                {
                    return Unauthorized(new
                    {
                        message = "Unable to identify the current user."
                    });
                }

                var task = await _taskService.AssignAsync(
                    tenantId,
                    projectId,
                    taskId,
                    userId,
                    request);

                return Ok(task);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to assign task {TaskId} for tenant {TenantId}, project {ProjectId}",
                    taskId,
                    tenantId,
                    projectId);

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                });
            }
        }
    }
}
