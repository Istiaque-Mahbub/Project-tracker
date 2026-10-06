using DeveloperProjectTracker.API.DTOs.Labels;
using DeveloperProjectTracker.API.Services.Labels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route(
        "api/tenants/{tenantId:guid}/projects/{projectId:guid}/labels")]
    [Authorize]
    public class LabelController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public LabelController(
            ILabelService labelService)
        {
            _labelService = labelService;
        }


        // =========================================================
        // CREATE LABEL
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<LabelResponseDto>> Create(
            Guid tenantId,
            Guid projectId,
            CreateLabelRequestDto request)
        {
            try
            {
                var label = await _labelService.CreateAsync(
                    tenantId,
                    projectId,
                    request);

                return Ok(label);
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new
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
        // GET ALL LABELS
        // =========================================================

        [HttpGet]
        public async Task<ActionResult<List<LabelResponseDto>>>
            GetAll(
                Guid tenantId,
                Guid projectId)
        {
            try
            {
                var labels = await _labelService.GetAllAsync(
                    tenantId,
                    projectId);

                return Ok(labels);
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
        // GET SINGLE LABEL
        // =========================================================

        [HttpGet("{labelId:guid}")]
        public async Task<ActionResult<LabelResponseDto>>
            GetById(
                Guid tenantId,
                Guid projectId,
                Guid labelId)
        {
            try
            {
                var label = await _labelService.GetByIdAsync(
                    tenantId,
                    projectId,
                    labelId);

                return Ok(label);
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
        // UPDATE LABEL
        // =========================================================

        [HttpPut("{labelId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<LabelResponseDto>>
            Update(
                Guid tenantId,
                Guid projectId,
                Guid labelId,
                UpdateLabelRequestDto request)
        {
            try
            {
                var label = await _labelService.UpdateAsync(
                    tenantId,
                    projectId,
                    labelId,
                    request);

                return Ok(label);
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new
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
        // DELETE LABEL
        // =========================================================

        [HttpDelete("{labelId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<IActionResult> Delete(
            Guid tenantId,
            Guid projectId,
            Guid labelId)
        {
            try
            {
                await _labelService.DeleteAsync(
                    tenantId,
                    projectId,
                    labelId);

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

        // =========================================================
        // ADD LABEL TO TASK
        // =========================================================

        [HttpPost("tasks/{taskId:guid}/{labelId:guid}")]
        public async Task<ActionResult<TaskLabelResponseDto>>
            AddToTask(
                Guid tenantId,
                Guid projectId,
                Guid taskId,
                Guid labelId)
        {
            try
            {
                var result =
                    await _labelService.AddToTaskAsync(
                        tenantId,
                        projectId,
                        taskId,
                        labelId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
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
        // GET TASK LABELS
        // =========================================================

        [HttpGet("tasks/{taskId:guid}")]
        public async Task<
            ActionResult<List<TaskLabelResponseDto>>>
            GetTaskLabels(
                Guid tenantId,
                Guid projectId,
                Guid taskId)
        {
            try
            {
                var labels =
                    await _labelService.GetTaskLabelsAsync(
                        tenantId,
                        projectId,
                        taskId);

                return Ok(labels);
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
        // REMOVE LABEL FROM TASK
        // =========================================================

        [HttpDelete(
            "tasks/{taskId:guid}/{labelId:guid}")]
        public async Task<IActionResult> RemoveFromTask(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid labelId)
        {
            try
            {
                await _labelService.RemoveFromTaskAsync(
                    tenantId,
                    projectId,
                    taskId,
                    labelId);

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
