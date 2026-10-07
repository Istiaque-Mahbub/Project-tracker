using System.Security.Claims;
using DeveloperProjectTracker.API.DTOs.Projects;
using DeveloperProjectTracker.API.Services.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route("api/tenants/{tenantId:guid}/projects")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(
            IProjectService projectService)
        {
            _projectService = projectService;
        }

        // =========================================================
        // CREATE PROJECT
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<ProjectResponseDto>> Create(
            Guid tenantId,
            CreateProjectRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var project =
                    await _projectService.CreateAsync(
                        tenantId,
                        userId,
                        request);

                return CreatedAtAction(
                    nameof(GetById),
                    new
                    {
                        tenantId,
                        projectId = project.Id
                    },
                    project);
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
        // GET ALL PROJECTS
        // =========================================================

        [HttpGet]
        public async Task<ActionResult<List<ProjectResponseDto>>> GetAll(
            Guid tenantId)
        {
            try
            {
                var projects =
                    await _projectService.GetAllAsync(
                        tenantId);

                return Ok(projects);
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
        // GET SINGLE PROJECT
        // =========================================================

        [HttpGet("{projectId:guid}")]
        public async Task<ActionResult<ProjectResponseDto>> GetById(
            Guid tenantId,
            Guid projectId)
        {
            try
            {
                var project =
                    await _projectService.GetByIdAsync(
                        tenantId,
                        projectId);

                return Ok(project);
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
        // UPDATE PROJECT
        // =========================================================

        [HttpPut("{projectId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<ProjectResponseDto>> Update(
            Guid tenantId,
            Guid projectId,
            UpdateProjectRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var project =
                    await _projectService.UpdateAsync(
                        tenantId,
                        projectId,
                        userId,
                        request);

                return Ok(project);
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
        // DELETE PROJECT
        // =========================================================

        [HttpDelete("{projectId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<IActionResult> Delete(
            Guid tenantId,
            Guid projectId)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _projectService.DeleteAsync(
                    tenantId,
                    projectId,
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
            var userIdValue =
                User.FindFirstValue(
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