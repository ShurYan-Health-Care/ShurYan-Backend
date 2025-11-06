using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.Identity
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; } = string.Empty; // The refresh token string (should be hashed before storage in production)

        public Guid UserId { get; set; } // User ID this token belongs to

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // When the token was created

        public DateTime ExpiresAt { get; set; } // When the token expires

        public bool IsRevoked { get; set; } = false; // Whether the token has been revoked (invalidated)

        public DateTime? RevokedAt { get; set; } // When the token was revoked

        public string? CreatedByIp { get; set; } // IP address from which the token was created

        public string? RevokedByIp { get; set; } // IP address from which the token was revoked

        public string? RevocationReason { get; set; } // Reason for revocation ("User logout", "Security concern")

        public virtual User User { get; set; } = null!; // Navigation property to User

        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt; // Check if the refresh token is currently active (not expired and not revoked)

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt; // Check if the refresh token has expired
    }
}