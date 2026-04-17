using Shuryan.Core.Entities.System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ChatRepositories
{
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        Task<Conversation?> GetUserActiveConversationAsync(Guid userId);
        Task<Conversation?> GetConversationWithMessagesAsync(Guid conversationId, int lastMessagesCount = 10);
    }
}
