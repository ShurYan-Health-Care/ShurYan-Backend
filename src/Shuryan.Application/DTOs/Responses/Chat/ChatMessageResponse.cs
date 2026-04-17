using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Chat
{
    /// <summary>
    /// Response لرسالة من الـ AI Bot
    /// </summary>
    public class ChatMessageResponse
    {
        /// <summary>
        /// معرف المحادثة
        /// </summary>
        public Guid ConversationId { get; set; }

        /// <summary>
        /// معرف الرسالة
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// رد الـ AI Bot
        /// </summary>
        public string Reply { get; set; } = string.Empty;

        /// <summary>
        /// اقتراحات سريعة للمستخدم
        /// </summary>
        public List<string>? Suggestions { get; set; }

        /// <summary>
        /// Actions يقدر الـ Frontend ينفذها
        /// </summary>
        public List<ChatActionDto>? Actions { get; set; }

        /// <summary>
        /// تاريخ الرسالة
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Action يقدر الـ Frontend ينفذه
    /// </summary>
    public class ChatActionDto
    {
        /// <summary>
        /// نوع الـ Action (navigate, open-modal, filter, etc.)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Route للـ Navigation
        /// </summary>
        public string? Route { get; set; }

        /// <summary>
        /// بيانات إضافية للـ Action
        /// </summary>
        public Dictionary<string, object>? Data { get; set; }

        /// <summary>
        /// Label للـ Action (للعرض للمستخدم)
        /// </summary>
        public string? Label { get; set; }
    }
}
