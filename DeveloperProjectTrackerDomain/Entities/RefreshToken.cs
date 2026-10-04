using DeveloperProjectTrackerDomain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeveloperProjectTrackerDomain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public Guid? TenantId { get; set; }

        public TenantRole? TenantRole { get; set; }

        public User User { get; set; } = null!;

        public Tenant? Tenant { get; set; }
    }
}
