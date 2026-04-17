using System;

namespace Shuryan.Application.DTOs.Responses.Chat
{
    /// <summary>
    /// Response لقائمة المحادثات (بدون الرسائل)
    /// </summary>
    public class ConversationListItemResponse
    {
        /// <summary>
        /// معرف المحادثة
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// عنوان المحادثة
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// آخر رسالة
        /// </summary>
        public string? LastMessage { get; set; }

        /// <summary>
        /// تاريخ آخر رسالة
        /// </summary>
        public DateTime? LastMessageAt { get; set; }

        /// <summary>
        /// هل المحادثة نشطة
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تاريخ الإنشاء
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// عدد الرسائل في المحادثة
        /// </summary>
        public int MessageCount { get; set; }
    }
}
