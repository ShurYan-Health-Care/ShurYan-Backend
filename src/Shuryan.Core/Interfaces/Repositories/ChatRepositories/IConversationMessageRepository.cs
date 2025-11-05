using Shuryan.Core.Entities.System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ChatRepositories
{
    /// <summary>
    /// Repository للتعامل مع رسائل المحادثات
    /// </summary>
    public interface IConversationMessageRepository : IGenericRepository<ConversationMessage>
    {
        /// <summary>
        /// جيب كل الرسائل الخاصة بمحادثة معينة
        /// </summary>
        Task<IEnumerable<ConversationMessage>> GetConversationMessagesAsync(Guid conversationId);

        /// <summary>
        /// جيب آخر N رسالة من محادثة (للـ Context)
        /// </summary>
        Task<IEnumerable<ConversationMessage>> GetRecentMessagesAsync(Guid conversationId, int count = 10);

        /// <summary>
        /// احسب إجمالي الـ Tokens المستخدمة في محادثة
        /// </summary>
        Task<int> GetTotalTokensUsedAsync(Guid conversationId);

        /// <summary>
        /// احسب متوسط وقت الاستجابة في محادثة
        /// </summary>
        Task<double> GetAverageResponseTimeAsync(Guid conversationId);

        /// <summary>
        /// امسح كل رسائل محادثة معينة
        /// </summary>
        Task DeleteConversationMessagesAsync(Guid conversationId);
    }
}
