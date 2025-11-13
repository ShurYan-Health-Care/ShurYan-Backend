using Shuryan.Core.Entities.System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ChatRepositories
{
    public interface IConversationMessageRepository : IGenericRepository<ConversationMessage>
    {
        Task<IEnumerable<ConversationMessage>> GetConversationMessagesPagedAsync(Guid conversationId, int skip, int take);
        Task<int> GetConversationMessageCountAsync(Guid conversationId);
        Task DeleteConversationMessagesAsync(Guid conversationId);
    }
}
