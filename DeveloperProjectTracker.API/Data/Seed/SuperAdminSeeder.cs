

using DeveloperProjectTrackerDomain.Entities;
using DeveloperProjectTrackerDomain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Data.Seed
{
    public static class SuperAdminSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            IConfiguration configuration)
        {
            // Check if a SuperAdmin already exists
            var superAdminExists = await context.Users
                .AnyAsync(u => u.Role == UserRole.SuperAdmin);

            if (superAdminExists)
            {
                return;
            }

            // Read SuperAdmin settings from configuration
            var fullName = configuration["SuperAdmin:FullName"];
            var email = configuration["SuperAdmin:Email"];
            var password = configuration["SuperAdmin:Password"];

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "SuperAdmin configuration is missing. " +
                    "Please configure SuperAdmin:FullName, " +
                    "SuperAdmin:Email and SuperAdmin:Password.");
            }

            email = email.Trim().ToLowerInvariant();

            // Check whether the email is already being used
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser is not null)
            {
                throw new InvalidOperationException(
                    $"Cannot create SuperAdmin. " +
                    $"The email '{email}' is already registered.");
            }

            // Create SuperAdmin
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName.Trim(),
                Email = email,
                Role = UserRole.SuperAdmin,
                CreatedAt = DateTime.UtcNow
            };

            // Hash password using the same password hasher
            // used by AuthService
            user.PasswordHash = passwordHasher.HashPassword(
                user,
                password);

            context.Users.Add(user);

            await context.SaveChangesAsync();
        }
    }
}

