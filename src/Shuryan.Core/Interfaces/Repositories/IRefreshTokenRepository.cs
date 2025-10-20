using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for RefreshToken operations
    /// </summary>
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        /// <summary>
        /// Finds a refresh token by its token string
        /// </summary>
        Task<RefreshToken?> GetByTokenAsync(string token);

        /// <summary>
        /// Gets all refresh tokens for a specific user
        /// </summary>
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Gets all active (non-revoked, non-expired) tokens for a user
        /// </summary>
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);

        /// <summary>
        /// Revokes a specific refresh token
        /// </summary>
        Task RevokeTokenAsync(string token, string? reason = null, string? revokedByIp = null);

        /// <summary>
        /// Revokes all refresh tokens for a specific user (useful for logout from all devices)
        /// </summary>
        Task RevokeAllUserTokensAsync(Guid userId, string? reason = null);

        /// <summary>
        /// Deletes all expired refresh tokens (for cleanup jobs)
        /// </summary>
        Task DeleteExpiredTokensAsync();

        /// <summary>
        /// Checks if a refresh token exists and is active
        /// </summary>
        Task<bool> IsTokenActiveAsync(string token);
    }
}