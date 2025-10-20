using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ShuryanDbContext context) : base(context) { }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(rt => rt.UserId == userId
                    && !rt.IsRevoked
                    && rt.ExpiresAt > now)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task RevokeTokenAsync(string token, string? reason = null, string? revokedByIp = null)
        {
            var refreshToken = await GetByTokenAsync(token);

            if (refreshToken != null && !refreshToken.IsRevoked)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevocationReason = reason;
                refreshToken.RevokedByIp = revokedByIp;

                Update(refreshToken);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, string? reason = null)
        {
            var activeTokens = await GetActiveTokensByUserIdAsync(userId);

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevocationReason = reason ?? "User logout from all devices";

                Update(token);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteExpiredTokensAsync()
        {
            var now = DateTime.UtcNow;
            var expiredTokens = await _dbSet
                .Where(rt => rt.ExpiresAt < now)
                .ToListAsync();

            foreach (var token in expiredTokens)
            {
                Delete(token);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTokenActiveAsync(string token)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .AnyAsync(rt => rt.Token == token
                    && !rt.IsRevoked
                    && rt.ExpiresAt > now);
        }
    }
}