using DeveloperProjectTracker.API.DTOs.Comments;
using DeveloperProjectTracker.API.Services.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route(
        "api/tenants/{tenantId:guid}/projects/{projectId:guid}/tasks/{taskId:guid}/comments")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(
            ICommentService commentService)
        {
            _commentService = commentService;
        }

        // =========================================================
        // CREATE COMMENT
        // =========================================================

        [HttpPost]
        public async Task<ActionResult<CommentResponseDto>> Create(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            CreateCommentRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var comment =
                    await _commentService.CreateAsync(
                        tenantId,
                        projectId,
                        taskId,
                        userId,
                        request);

                return Ok(comment);
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
        // GET ALL COMMENTS
        // =========================================================

        [HttpGet]
        public async Task<
            ActionResult<List<CommentResponseDto>>> GetAll(
            Guid tenantId,
            Guid projectId,
            Guid taskId)
        {
            try
            {
                var comments =
                    await _commentService.GetAllAsync(
                        tenantId,
                        projectId,
                        taskId);

                return Ok(comments);
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
        // GET SINGLE COMMENT
        // =========================================================

        [HttpGet("{commentId:guid}")]
        public async Task<ActionResult<CommentResponseDto>>
            GetById(
                Guid tenantId,
                Guid projectId,
                Guid taskId,
                Guid commentId)
        {
            try
            {
                var comment =
                    await _commentService.GetByIdAsync(
                        tenantId,
                        projectId,
                        taskId,
                        commentId);

                return Ok(comment);
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
        // UPDATE COMMENT
        // =========================================================

        [HttpPut("{commentId:guid}")]
        public async Task<ActionResult<CommentResponseDto>>
            Update(
                Guid tenantId,
                Guid projectId,
                Guid taskId,
                Guid commentId,
                UpdateCommentRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var comment =
                    await _commentService.UpdateAsync(
                        tenantId,
                        projectId,
                        taskId,
                        commentId,
                        userId,
                        request);

                return Ok(comment);
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
        // DELETE COMMENT
        // =========================================================

        [HttpDelete("{commentId:guid}")]
        public async Task<IActionResult> Delete(
            Guid tenantId,
            Guid projectId,
            Guid taskId,
            Guid commentId)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _commentService.DeleteAsync(
                    tenantId,
                    projectId,
                    taskId,
                    commentId,
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
