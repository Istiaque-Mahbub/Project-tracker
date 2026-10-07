using System.Security.Claims;
using DeveloperProjectTracker.API.DTOs.ProjectMembers;
using DeveloperProjectTracker.API.Services.ProjectMembers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route("api/tenants/{tenantId:guid}/projects/{projectId:guid}/members")]
    [Authorize]
    public class ProjectMemberController : ControllerBase
    {
        private readonly IProjectMemberService _projectMemberService;

        public ProjectMemberController(
            IProjectMemberService projectMemberService)
        {
            _projectMemberService = projectMemberService;
        }

        // =========================================================
        // ADD PROJECT MEMBER
        // POST:
        // /api/tenants/{tenantId}/projects/{projectId}/members
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<ProjectMemberResponseDto>> AddMember(
            Guid tenantId,
            Guid projectId,
            AddProjectMemberRequestDto request)
        {
            try
            {
                // Get currently authenticated user
                var userIdClaim = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdClaim, out var actorUserId))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid authenticated user."
                    });
                }

                var member =
                    await _projectMemberService.AddMemberAsync(
                        tenantId,
                        projectId,
                        actorUserId,
                        request);

                return Ok(member);
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
        // GET ALL PROJECT MEMBERS
        // GET:
        // /api/tenants/{tenantId}/projects/{projectId}/members
        // =========================================================

        [HttpGet]
        public async Task<ActionResult<List<ProjectMemberResponseDto>>> GetMembers(
            Guid tenantId,
            Guid projectId)
        {
            try
            {
                var members =
                    await _projectMemberService.GetMembersAsync(
                        tenantId,
                        projectId);

                return Ok(members);
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
        // GET SINGLE PROJECT MEMBER
        // GET:
        // /api/tenants/{tenantId}/projects/{projectId}/members/{userId}
        // =========================================================

        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<ProjectMemberResponseDto>> GetMember(
            Guid tenantId,
            Guid projectId,
            Guid userId)
        {
            try
            {
                var member =
                    await _projectMemberService.GetMemberAsync(
                        tenantId,
                        projectId,
                        userId);

                return Ok(member);
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
        // REMOVE PROJECT MEMBER
        // DELETE:
        // /api/tenants/{tenantId}/projects/{projectId}/members/{userId}
        // =========================================================

        [HttpDelete("{userId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<IActionResult> RemoveMember(
            Guid tenantId,
            Guid projectId,
            Guid userId)
        {
            try
            {
                // Get currently authenticated user
                var actorUserIdClaim = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(
                        actorUserIdClaim,
                        out var actorUserId))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid authenticated user."
                    });
                }

                await _projectMemberService.RemoveMemberAsync(
                    tenantId,
                    projectId,
                    userId,
                    actorUserId);

                return NoContent();
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
    }
}