using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums.Chat;
using System;
using System.Text.Json;

namespace Shuryan.Core.Entities.System
{
    /// <summary>
    /// رسالة واحدة جوه المحادثة
    /// ممكن تكون من الـ User أو من الـ AI Bot
    /// </summary>
    public class ConversationMessage : AuditableEntity
    {
        /// <summary>
        /// المحادثة اللي الرسالة دي تابعة ليها
        /// </summary>
        public Guid ConversationId { get; set; }

        /// <summary>
        /// دور المرسل (User أو Assistant)
        /// </summary>
        public MessageRole Role { get; set; }

        /// <summary>
        /// محتوى الرسالة
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// اقتراحات سريعة للمستخدم (JSON Array)
        /// مثلاً: ["شوف الدكاترة المتاحين", "احجز موعد دلوقتي"]
        /// </summary>
        public string? SuggestionsJson { get; set; }

        /// <summary>
        /// Actions يقدر الـ Frontend ينفذها (JSON Array)
        /// مثلاً: [{"type": "navigate", "route": "/patient/search"}]
        /// </summary>
        public string? ActionsJson { get; set; }

        /// <summary>
        /// Context إضافي عن الرسالة (JSON Object)
        /// مثلاً: {"currentPage": "search-doctors", "doctorId": "uuid"}
        /// </summary>
        public string? ContextJson { get; set; }

        /// <summary>
        /// عدد الـ Tokens المستخدمة في الرسالة دي (للإحصائيات)
        /// </summary>
        public int? TokenCount { get; set; }

        /// <summary>
        /// وقت الاستجابة بالـ milliseconds (للإحصائيات)
        /// </summary>
        public int? ResponseTimeMs { get; set; }

        /// <summary>
        /// Navigation property للمحادثة
        /// </summary>
        public virtual Conversation Conversation { get; set; } = null!;

        // Helper methods للتعامل مع الـ JSON
        public string[]? GetSuggestions()
        {
            if (string.IsNullOrEmpty(SuggestionsJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<string[]>(SuggestionsJson);
            }
            catch
            {
                return null;
            }
        }

        public void SetSuggestions(string[]? suggestions)
        {
            SuggestionsJson = suggestions != null 
                ? JsonSerializer.Serialize(suggestions) 
                : null;
        }
    }
}
