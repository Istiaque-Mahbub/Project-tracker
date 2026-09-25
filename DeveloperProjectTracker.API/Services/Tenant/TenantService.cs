using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Tenant;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.EntityFrameworkCore;



namespace DeveloperProjectTracker.API.Services.Tenants
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;

        public TenantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TenantResponseDto> CreateAsync(
            CreateTenantRequestDto request)
        {
            var name = request.Name.Trim();
            var slug = request.Slug.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Tenant name is required.");
            }

            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new ArgumentException(
                    "Tenant slug is required.");
            }

            var slugExists = await _context.Tenants
                .AnyAsync(t => t.Slug == slug);

            if (slugExists)
            {
                throw new InvalidOperationException(
                    "A tenant with this slug already exists.");
            }

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Tenants.Add(tenant);

            await _context.SaveChangesAsync();

            return MapToResponse(tenant);
        }


        public async Task<List<TenantResponseDto>> GetAllAsync()
        {
            return await _context.Tenants
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new TenantResponseDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<TenantResponseDto> GetByIdAsync(Guid id)
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant is null)
            {
                throw new KeyNotFoundException(
                    "Tenant not found.");
            }

            return MapToResponse(tenant);
        }


       

        public async Task<TenantResponseDto> UpdateAsync(
            Guid id,
            UpdateTenantRequestDto request)
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant is null)
            {
                throw new KeyNotFoundException(
                    "Tenant not found.");
            }

            var name = request.Name.Trim();
            var slug = request.Slug.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Tenant name is required.");
            }

            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new ArgumentException(
                    "Tenant slug is required.");
            }

            var slugExists = await _context.Tenants
                .AnyAsync(t =>
                    t.Id != id &&
                    t.Slug == slug);

            if (slugExists)
            {
                throw new InvalidOperationException(
                    "A tenant with this slug already exists.");
            }

            tenant.Name = name;
            tenant.Slug = slug;

            await _context.SaveChangesAsync();

            return MapToResponse(tenant);
        }

        public async Task ActivateAsync(Guid id)
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant is null)
            {
                throw new KeyNotFoundException(
                    "Tenant not found.");
            }

            tenant.IsActive = true;

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(Guid id)
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant is null)
            {
                throw new KeyNotFoundException(
                    "Tenant not found.");
            }

            tenant.IsActive = false;

            await _context.SaveChangesAsync();
        }

        private static TenantResponseDto MapToResponse(
          Tenant tenant)
        {
            return new TenantResponseDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt
            };
        }
    }
}
