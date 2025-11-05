using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Interfaces.Repositories.ChatRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Chat
{
    /// <summary>
    /// Implementation للـ Conversation Repository
    /// </summary>
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(ShuryanDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId, bool activeOnly = true)
        {
            var query = _dbSet
                .Where(c => c.UserId == userId);

            if (activeOnly)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Conversation?> GetLatestActiveConversationAsync(Guid userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId && c.IsActive)
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Conversation?> GetConversationWithMessagesAsync(Guid conversationId)
        {
            return await _dbSet
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }

        public async Task ArchiveConversationAsync(Guid conversationId)
        {
            var conversation = await _dbSet.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.IsActive = false;
                conversation.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(conversation);
            }
        }

        public async Task DeleteOldConversationsAsync(int daysOld)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            
            var oldConversations = await _dbSet
                .Where(c => c.CreatedAt < cutoffDate && !c.IsActive)
                .ToListAsync();

            _dbSet.RemoveRange(oldConversations);
        }
    }
}
