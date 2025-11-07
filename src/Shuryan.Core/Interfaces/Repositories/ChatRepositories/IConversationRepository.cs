using Shuryan.Core.Entities.System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ChatRepositories
{
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        /// <summary>
        /// جيب المحادثة النشطة للمستخدم (كل مستخدم عنده محادثة واحدة)
        /// </summary>
        Task<Conversation?> GetUserActiveConversationAsync(Guid userId);

        /// <summary>
        /// جيب محادثة مع آخر رسائلها
        /// </summary>
        Task<Conversation?> GetConversationWithMessagesAsync(Guid conversationId, int lastMessagesCount = 10);
    }
}
