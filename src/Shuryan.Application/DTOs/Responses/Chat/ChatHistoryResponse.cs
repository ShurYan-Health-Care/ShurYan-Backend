using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Chat
{
    /// <summary>
    /// Response لتاريخ المحادثة مع Pagination
    /// </summary>
    public class ChatHistoryResponse
    {
        /// <summary>
        /// معرف المحادثة
        /// </summary>
        public Guid ConversationId { get; set; }

        /// <summary>
        /// قائمة الرسائل
        /// </summary>
        public List<ChatMessageDto> Messages { get; set; } = new();

        /// <summary>
        /// معلومات الـ Pagination
        /// </summary>
        public PaginationInfo Pagination { get; set; } = new();

        /// <summary>
        /// هل في رسائل أقدم متاحة؟
        /// </summary>
        public bool HasMore { get; set; }
    }

    /// <summary>
    /// رسالة واحدة في المحادثة
    /// </summary>
    public class ChatMessageDto
    {
        /// <summary>
        /// معرف الرسالة
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// دور المرسل (User أو Assistant)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// محتوى الرسالة
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// اقتراحات سريعة (للـ Assistant فقط)
        /// </summary>
        public List<string>? Suggestions { get; set; }

        /// <summary>
        /// Actions متاحة (للـ Assistant فقط)
        /// </summary>
        public List<ChatActionDto>? Actions { get; set; }

        /// <summary>
        /// تاريخ الرسالة
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// معلومات الـ Pagination
    /// </summary>
    public class PaginationInfo
    {
        /// <summary>
        /// رقم الصفحة الحالية
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// عدد العناصر في الصفحة
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// إجمالي عدد الرسائل
        /// </summary>
        public int TotalMessages { get; set; }

        /// <summary>
        /// إجمالي عدد الصفحات
        /// </summary>
        public int TotalPages { get; set; }
    }
}
