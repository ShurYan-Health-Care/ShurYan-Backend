using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Shared.Configurations
{
    /// <summary>
    /// JWT Token configuration settings
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Secret key used to sign JWT tokens (should be stored in environment variables in production)
        /// Minimum 256 bits (32 characters) for HS256
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Token issuer - typically your API URL
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Token audience - typically your client application URL
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration in minutes (default: 15 minutes)
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 15;

        /// <summary>
        /// Refresh token expiration in days (default: 7 days)
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// Validate token issuer
        /// </summary>
        public bool ValidateIssuer { get; set; } = true;

        /// <summary>
        /// Validate token audience
        /// </summary>
        public bool ValidateAudience { get; set; } = true;

        /// <summary>
        /// Validate token lifetime
        /// </summary>
        public bool ValidateLifetime { get; set; } = true;

        /// <summary>
        /// Validate issuer signing key
        /// </summary>
        public bool ValidateIssuerSigningKey { get; set; } = true;

        /// <summary>
        /// Clock skew for token expiration (in minutes)
        /// Allows for minor time differences between servers
        /// </summary>
        public int ClockSkewMinutes { get; set; } = 5;
    }
}