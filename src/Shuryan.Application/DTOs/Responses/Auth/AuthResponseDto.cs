using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Auth
{
    /// <summary>
    /// Response DTO for successful authentication
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// JWT access token (short-lived)
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token (long-lived)
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration time in UTC
        /// </summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// Refresh token expiration time in UTC
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }

        /// <summary>
        /// Token type (always "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// User information
        /// </summary>
        public UserInfoDto User { get; set; } = null!;
    }
}