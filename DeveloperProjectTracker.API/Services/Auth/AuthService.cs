using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.DTOs.Auth;
using DeveloperProjectTrackerDomain.Entities;
using DeveloperProjectTrackerDomain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeveloperProjectTracker.API.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(
            ApplicationDbContext context,
            IJwtService jwtService,
            IPasswordHasher<User> passwordHasher,
            IConfiguration configuration)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(
            RegisterRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser is not null)
            {
                throw new InvalidOperationException(
                    "A user with this email already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Email = email,
                Role = UserRole.Developer,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return await CreateAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(
            LoginRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            return await CreateAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(
            RefreshTokenRequestDto request)
        {
            var storedToken = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Token == request.RefreshToken);

            if (storedToken is null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token.");
            }

            if (storedToken.RevokedAt.HasValue)
            {
                throw new UnauthorizedAccessException(
                    "Refresh token has been revoked.");
            }

            if (storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException(
                    "Refresh token has expired.");
            }

            // Revoke the old refresh token
            storedToken.RevokedAt = DateTime.UtcNow;

            // Generate new tokens
            var newAccessToken =
                _jwtService.GenerateAccessToken(storedToken.User);

            var newRefreshToken =
                _jwtService.GenerateRefreshToken();

            var refreshTokenDays =
                _configuration.GetValue<int>(
                    "Jwt:RefreshTokenExpirationDays");

            var newStoredToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = storedToken.UserId,
                Token = newRefreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
            };

            _context.RefreshTokens.Add(newStoredToken);

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = storedToken.User.Id,
                FullName = storedToken.User.FullName,
                Email = storedToken.User.Email,
                Role = storedToken.User.Role.ToString(),
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>(
                        "Jwt:AccessTokenExpirationMinutes"))
            };
        }

        private async Task<AuthResponseDto> CreateAuthResponseAsync(
            User user)
        {
            var accessToken =
                _jwtService.GenerateAccessToken(user);

            var refreshToken =
                _jwtService.GenerateRefreshToken();

            var refreshTokenDays =
                _configuration.GetValue<int>(
                    "Jwt:RefreshTokenExpirationDays");

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>(
                        "Jwt:AccessTokenExpirationMinutes"))
            };
        }
    }
}
