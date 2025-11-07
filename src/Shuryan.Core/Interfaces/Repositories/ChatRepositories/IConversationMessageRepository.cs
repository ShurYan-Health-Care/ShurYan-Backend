using Shuryan.Core.Entities.System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ChatRepositories
{
    public interface IConversationMessageRepository : IGenericRepository<ConversationMessage>
    {
        /// <summary>
        /// جيب رسائل محادثة مع Pagination (من الأحدث للأقدم)
        /// </summary>
        Task<IEnumerable<ConversationMessage>> GetConversationMessagesPagedAsync(
            Guid conversationId, 
            int skip, 
            int take);

        /// <summary>
        /// جيب عدد رسائل محادثة
        /// </summary>
        Task<int> GetConversationMessageCountAsync(Guid conversationId);

        /// <summary>
        /// امسح كل رسائل محادثة
        /// </summary>
        Task DeleteConversationMessagesAsync(Guid conversationId);
    }
}
