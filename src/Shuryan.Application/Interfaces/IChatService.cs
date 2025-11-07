using Shuryan.Application.DTOs.Requests.Chat;
using Shuryan.Application.DTOs.Responses.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service للتعامل مع المحادثة والـ AI Bot
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// إرسال رسالة للـ AI Bot
        /// يتم إنشاء محادثة تلقائياً إذا لم تكن موجودة
        /// </summary>
        Task<ChatMessageResponse?> SendMessageAsync(Guid userId, string userRole, SendMessageRequest request);

        /// <summary>
        /// جيب تاريخ المحادثة مع Pagination
        /// </summary>
        /// <param name="userId">معرف المستخدم</param>
        /// <param name="pageNumber">رقم الصفحة (1-based)</param>
        /// <param name="pageSize">عدد الرسائل في الصفحة</param>
        Task<ChatHistoryResponse?> GetChatHistoryAsync(Guid userId, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// امسح كل رسائل المحادثة (ابدأ من جديد)
        /// </summary>
        Task<bool> ClearUserChatAsync(Guid userId);
    }
}
