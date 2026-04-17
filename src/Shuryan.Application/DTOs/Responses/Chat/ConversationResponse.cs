using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Chat
{
    /// <summary>
    /// Response لمحادثة كاملة مع كل رسائلها
    /// </summary>
    public class ConversationResponse
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
        /// الرسائل
        /// </summary>
        public List<ConversationMessageDto>? Messages { get; set; }
    }

    /// <summary>
    /// رسالة واحدة في المحادثة
    /// </summary>
    public class ConversationMessageDto
    {
        /// <summary>
        /// معرف الرسالة
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// دور المرسل (User أو Assistant)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// محتوى الرسالة
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// اقتراحات سريعة
        /// </summary>
        public List<string>? Suggestions { get; set; }

        /// <summary>
        /// Actions
        /// </summary>
        public List<ChatActionDto>? Actions { get; set; }

        /// <summary>
        /// تاريخ الإنشاء
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
