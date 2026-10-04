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

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<ProjectResponseDto>> Create(
            Guid tenantId,
            CreateProjectRequestDto request)
        {
            try
            {
                var project = await _projectService.CreateAsync(
                    tenantId,
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
                        message = "An unexpected error occurred."
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
                var projects = await _projectService.GetAllAsync(
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
                        message = "An unexpected error occurred."
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
                var project = await _projectService.GetByIdAsync(
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
                        message = "An unexpected error occurred."
                    });
            }
        }

        [HttpPut("{projectId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<ProjectResponseDto>> Update(
    Guid tenantId,
    Guid projectId,
    UpdateProjectRequestDto request)
        {
            try
            {
                var project = await _projectService.UpdateAsync(
                    tenantId,
                    projectId,
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

        [HttpDelete("{projectId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<IActionResult> Delete(
    Guid tenantId,
    Guid projectId)
        {
            try
            {
                await _projectService.DeleteAsync(
                    tenantId,
                    projectId);

                return NoContent();
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
                        message = "An unexpected error occurred."
                    });
            }
        }

    }
}
