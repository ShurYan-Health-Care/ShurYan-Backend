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
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(ShuryanDbContext context) : base(context)
        {
        }

        public async Task<Conversation?> GetUserActiveConversationAsync(Guid userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId && c.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<Conversation?> GetConversationWithMessagesAsync(
            Guid conversationId, 
            int lastMessagesCount = 10)
        {
            return await _dbSet
                .Include(c => c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(lastMessagesCount))
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }
    }
}
