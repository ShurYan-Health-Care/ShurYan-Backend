using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Service للتعامل مع Gemini AI API
    /// </summary>
    public interface IGeminiAIService
    {
        /// <summary>
        /// إرسال رسالة للـ Gemini AI والحصول على رد
        /// </summary>
        /// <param name="userMessage">رسالة المستخدم</param>
        /// <param name="conversationHistory">تاريخ المحادثة (للـ Context)</param>
        /// <param name="systemPrompt">System Prompt حسب دور المستخدم</param>
        /// <returns>رد الـ AI</returns>
        Task<GeminiResponse> SendMessageAsync(
            string userMessage, 
            List<ConversationHistoryItem>? conversationHistory = null,
            string? systemPrompt = null
        );

        /// <summary>
        /// جيب System Prompt حسب دور المستخدم
        /// </summary>
        string GetSystemPrompt(string userRole);
    }

    /// <summary>
    /// Response من Gemini AI
    /// </summary>
    public class GeminiResponse
    {
        /// <summary>
        /// رد الـ AI
        /// </summary>
        public string Reply { get; set; } = string.Empty;

        /// <summary>
        /// عدد الـ Tokens المستخدمة
        /// </summary>
        public int TokenCount { get; set; }

        /// <summary>
        /// وقت الاستجابة بالـ milliseconds
        /// </summary>
        public int ResponseTimeMs { get; set; }

        /// <summary>
        /// هل في خطأ
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// رسالة الخطأ
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// عنصر واحد في تاريخ المحادثة
    /// </summary>
    public class ConversationHistoryItem
    {
        /// <summary>
        /// دور المرسل (user أو assistant)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// محتوى الرسالة
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
