using DeveloperProjectTracker.API.Configuration;
using DeveloperProjectTrackerDomain.Entities;
using DeveloperProjectTrackerDomain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DeveloperProjectTracker.API.Services.Auth
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateAccessToken(
            User user,
            Guid? tenantId = null,
            TenantRole? tenantRole = null)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.NameIdentifier, user.FullName),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            // Add tenant information when the user
            // is accessing a specific tenant.
            if (tenantId.HasValue)
            {
                claims.Add(
                    new Claim(
                        "tenantId",
                        tenantId.Value.ToString()));
            }

            if (tenantRole.HasValue)
            {
                claims.Add(
                    new Claim(
                        "tenantRole",
                        tenantRole.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(
                _jwtSettings.AccessTokenExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes =
                RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
