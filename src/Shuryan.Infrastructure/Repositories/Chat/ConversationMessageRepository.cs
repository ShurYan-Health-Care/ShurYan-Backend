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
    public class ConversationMessageRepository : GenericRepository<ConversationMessage>, IConversationMessageRepository
    {
        public ConversationMessageRepository(ShuryanDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ConversationMessage>> GetConversationMessagesPagedAsync(
            Guid conversationId,
            int skip,
            int take)
        {
            return await _dbSet
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetConversationMessageCountAsync(Guid conversationId)
        {
            return await _dbSet
                .Where(m => m.ConversationId == conversationId)
                .CountAsync();
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
