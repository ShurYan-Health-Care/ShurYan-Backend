using Shuryan.Core.Entities.System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ChatRepositories
{
    /// <summary>
    /// Repository للتعامل مع المحادثات
    /// </summary>
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        /// <summary>
        /// جيب كل المحادثات الخاصة بـ User معين
        /// </summary>
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId, bool activeOnly = true);

        /// <summary>
        /// جيب آخر محادثة نشطة للـ User
        /// </summary>
        Task<Conversation?> GetLatestActiveConversationAsync(Guid userId);

        /// <summary>
        /// جيب محادثة مع كل الرسائل بتاعتها
        /// </summary>
        Task<Conversation?> GetConversationWithMessagesAsync(Guid conversationId);

        /// <summary>
        /// أرشف محادثة (خليها غير نشطة)
        /// </summary>
        Task ArchiveConversationAsync(Guid conversationId);

        /// <summary>
        /// امسح كل المحادثات القديمة (أكتر من X يوم)
        /// </summary>
        Task DeleteOldConversationsAsync(int daysOld);
    }
}
