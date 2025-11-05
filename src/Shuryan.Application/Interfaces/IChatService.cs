using Shuryan.Application.DTOs.Requests.Chat;
using Shuryan.Application.DTOs.Responses.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service للتعامل مع المحادثات والـ AI Bot
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// إرسال رسالة للـ AI Bot
        /// </summary>
        Task<ChatMessageResponse?> SendMessageAsync(Guid userId, string userRole, SendMessageRequest request);

        /// <summary>
        /// جيب محادثة معينة مع كل رسائلها
        /// </summary>
        Task<ConversationResponse?> GetConversationAsync(Guid conversationId, Guid userId);

        /// <summary>
        /// جيب كل محادثات المستخدم
        /// </summary>
        Task<IEnumerable<ConversationListItemResponse>> GetUserConversationsAsync(Guid userId, bool activeOnly = true);

        /// <summary>
        /// امسح محادثة
        /// </summary>
        Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId);

        /// <summary>
        /// أرشف محادثة (خليها غير نشطة)
        /// </summary>
        Task<bool> ArchiveConversationAsync(Guid conversationId, Guid userId);

        /// <summary>
        /// امسح كل رسائل محادثة
        /// </summary>
        Task<bool> ClearConversationMessagesAsync(Guid conversationId, Guid userId);
    }
}
