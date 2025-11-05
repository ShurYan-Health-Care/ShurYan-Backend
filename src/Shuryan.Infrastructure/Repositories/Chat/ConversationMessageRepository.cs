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
    /// Implementation للـ ConversationMessage Repository
    /// </summary>
    public class ConversationMessageRepository : GenericRepository<ConversationMessage>, IConversationMessageRepository
    {
        public ConversationMessageRepository(ShuryanDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ConversationMessage>> GetConversationMessagesAsync(Guid conversationId)
        {
            return await _dbSet
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ConversationMessage>> GetRecentMessagesAsync(Guid conversationId, int count = 10)
        {
            return await _dbSet
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .OrderBy(m => m.CreatedAt) // عكس الترتيب تاني عشان يكونوا من الأقدم للأحدث
                .ToListAsync();
        }

        public async Task<int> GetTotalTokensUsedAsync(Guid conversationId)
        {
            return await _dbSet
                .Where(m => m.ConversationId == conversationId && m.TokenCount.HasValue)
                .SumAsync(m => m.TokenCount ?? 0);
        }

        public async Task<double> GetAverageResponseTimeAsync(Guid conversationId)
        {
            var messages = await _dbSet
                .Where(m => m.ConversationId == conversationId && m.ResponseTimeMs.HasValue)
                .ToListAsync();

            if (!messages.Any())
                return 0;

            return messages.Average(m => m.ResponseTimeMs ?? 0);
        }

        public async Task DeleteConversationMessagesAsync(Guid conversationId)
        {
            var messages = await _dbSet
                .Where(m => m.ConversationId == conversationId)
                .ToListAsync();

            _dbSet.RemoveRange(messages);
        }
    }
}
