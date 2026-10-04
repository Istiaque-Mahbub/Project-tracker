using DeveloperProjectTracker.API.DTOs.Tenant;
using DeveloperProjectTracker.API.Services.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperProjectTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        // POST: api/Tenant
        [HttpPost]
        public async Task<ActionResult<TenantResponseDto>> Create(
            CreateTenantRequestDto request)
        {
            try
            {
                var tenant = await _tenantService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = tenant.Id },
                    tenant);
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
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while creating the tenant."
                });
            }
        }

        // GET: api/Tenant
        [HttpGet]
        public async Task<ActionResult<List<TenantResponseDto>>> GetAll()
        {
            try
            {
                var tenants = await _tenantService.GetAllAsync();

                return Ok(tenants);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while retrieving tenants."
                });
            }
        }

        // GET: api/Tenant/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TenantResponseDto>> GetById(
            Guid id)
        {
            try
            {
                var tenant = await _tenantService.GetByIdAsync(id);

                return Ok(tenant);
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
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while retrieving the tenant."
                });
            }
        }

        // PUT: api/Tenant/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TenantResponseDto>> Update(
            Guid id,
            UpdateTenantRequestDto request)
        {
            try
            {
                var tenant = await _tenantService.UpdateAsync(
                    id,
                    request);

                return Ok(tenant);
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
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while updating the tenant."
                });
            }
        }

        // PATCH: api/Tenant/{id}/activate
        [HttpPatch("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            try
            {
                await _tenantService.ActivateAsync(id);

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
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while activating the tenant."
                });
            }
        }

        // PATCH: api/Tenant/{id}/deactivate
        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            try
            {
                await _tenantService.DeactivateAsync(id);

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
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while deactivating the tenant."
                });
            }
        }
    }
}