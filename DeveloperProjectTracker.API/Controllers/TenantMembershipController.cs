using DeveloperProjectTracker.API.DTOs.Tenant;
using DeveloperProjectTracker.API.Services.Tenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route("api/tenants/{tenantId:guid}/members")]
    [Authorize]
    public class TenantMembershipController : ControllerBase
    {
        private readonly ITenantMembershipService _membershipService;

        public TenantMembershipController(
            ITenantMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        // =========================================================
        // ADD MEMBER
        // =========================================================

        [HttpPost]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<TenantMemberResponseDto>> AddMember(
            Guid tenantId,
            AddTenantMemberRequestDto request)
        {
            try
            {
                var member = await _membershipService.AddMemberAsync(
                    tenantId,
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
        // GET ALL MEMBERS
        // =========================================================

        [HttpGet]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<List<TenantMemberResponseDto>>> GetMembers(
            Guid tenantId)
        {
            try
            {
                var members = await _membershipService.GetMembersAsync(
                    tenantId);

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
        // GET SINGLE MEMBER
        // =========================================================

        [HttpGet("{userId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<TenantMemberResponseDto>> GetMember(
            Guid tenantId,
            Guid userId)
        {
            try
            {
                var member = await _membershipService.GetMemberAsync(
                    tenantId,
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
        // UPDATE MEMBER ROLE
        // =========================================================

        [HttpPut("{userId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<ActionResult<TenantMemberResponseDto>> UpdateRole(
            Guid tenantId,
            Guid userId,
            UpdateTenantMemberRoleRequestDto request)
        {
            try
            {
                var member = await _membershipService.UpdateRoleAsync(
                    tenantId,
                    userId,
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
        // REMOVE MEMBER
        // =========================================================

        [HttpDelete("{userId:guid}")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<IActionResult> RemoveMember(
            Guid tenantId,
            Guid userId)
        {
            try
            {
                await _membershipService.RemoveMemberAsync(
                    tenantId,
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
                        message = "An unexpected error occurred."
                    });
            }
        }
    }
}