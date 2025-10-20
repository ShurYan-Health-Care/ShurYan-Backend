using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.Services.Token
{
    /// <summary>
    /// Service for generating and validating JWT tokens
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates an access token for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="email">User email</param>
        /// <param name="roles">User roles</param>
        /// <param name="additionalClaims">Any additional claims to include</param>
        /// <returns>JWT access token string</returns>
        string GenerateAccessToken(
            Guid userId,
            string email,
            IEnumerable<string> roles,
            Dictionary<string, string>? additionalClaims = null);

        /// <summary>
        /// Generates a refresh token
        /// </summary>
        /// <returns>Secure random refresh token string</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validates a JWT token and returns the principal if valid
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>ClaimsPrincipal if valid, null otherwise</returns>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Extracts user ID from a JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID if found, null otherwise</returns>
        Guid? GetUserIdFromToken(string token);

        /// <summary>
        /// Extracts user email from a JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Email if found, null otherwise</returns>
        string? GetEmailFromToken(string token);

        /// <summary>
        /// Extracts all claims from a JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Collection of claims</returns>
        IEnumerable<Claim> GetClaimsFromToken(string token);

        /// <summary>
        /// Checks if a token has expired
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>True if expired, false otherwise</returns>
        bool IsTokenExpired(string token);
    }
}