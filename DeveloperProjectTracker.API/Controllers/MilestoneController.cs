using DeveloperProjectTracker.API.DTOs.Milestones;
using DeveloperProjectTracker.API.Services.Milestones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route(
        "api/tenants/{tenantId:guid}/projects/{projectId:guid}/milestones")]
    [Authorize]
    public class MilestoneController : ControllerBase
    {
        private readonly IMilestoneService _milestoneService;

        public MilestoneController(
            IMilestoneService milestoneService)
        {
            _milestoneService = milestoneService;
        }

        // =========================================================
        // CREATE MILESTONE
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<MilestoneResponseDto>> Create(
            Guid tenantId,
            Guid projectId,
            CreateMilestoneRequestDto request)
        {
            try
            {
                var milestone =
                    await _milestoneService.CreateAsync(
                        tenantId,
                        projectId,
                        request);

                return Ok(milestone);
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
                        message =
                            "An unexpected error occurred."
                    });
            }
        }

        // =========================================================
        // GET ALL MILESTONES
        // =========================================================

        [HttpGet]
        public async Task<
            ActionResult<List<MilestoneResponseDto>>> GetAll(
            Guid tenantId,
            Guid projectId)
        {
            try
            {
                var milestones =
                    await _milestoneService.GetAllAsync(
                        tenantId,
                        projectId);

                return Ok(milestones);
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
        // GET SINGLE MILESTONE
        // =========================================================

        [HttpGet("{milestoneId:guid}")]
        public async Task<ActionResult<MilestoneResponseDto>>
            GetById(
                Guid tenantId,
                Guid projectId,
                Guid milestoneId)
        {
            try
            {
                var milestone =
                    await _milestoneService.GetByIdAsync(
                        tenantId,
                        projectId,
                        milestoneId);

                return Ok(milestone);
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
        // UPDATE MILESTONE
        // =========================================================

        [HttpPut("{milestoneId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<MilestoneResponseDto>>
            Update(
                Guid tenantId,
                Guid projectId,
                Guid milestoneId,
                UpdateMilestoneRequestDto request)
        {
            try
            {
                var milestone =
                    await _milestoneService.UpdateAsync(
                        tenantId,
                        projectId,
                        milestoneId,
                        request);

                return Ok(milestone);
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
                        message =
                            "An unexpected error occurred."
                    });
            }
        }

        // =========================================================
        // DELETE MILESTONE
        // =========================================================

        [HttpDelete("{milestoneId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<IActionResult> Delete(
            Guid tenantId,
            Guid projectId,
            Guid milestoneId)
        {
            try
            {
                await _milestoneService.DeleteAsync(
                    tenantId,
                    projectId,
                    milestoneId);

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
                        message =
                            "An unexpected error occurred."
                    });
            }
        }
    }
}