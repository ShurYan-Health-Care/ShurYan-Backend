using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shuryan.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shuryan.Application.Services.AI
{
    /// <summary>
    /// Service للتعامل مع Gemini AI API
    /// بيستخدم Google Generative AI REST API
    /// </summary>
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiAIService> _logger;
        private readonly string _apiKey;
        private readonly string _modelName;

        public GeminiAIService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // جيب الـ API Key من الـ Configuration
            _apiKey = _configuration["GeminiAI:ApiKey"] 
                ?? throw new InvalidOperationException("Gemini API Key is not configured");

            // اسم الموديل (gemini-pro أو gemini-1.5-flash)
            _modelName = _configuration["GeminiAI:ModelName"] ?? "gemini-1.5-flash";

            _logger.LogInformation("✅ GeminiAIService initialized with model: {ModelName}", _modelName);
        }

        public async Task<GeminiResponse> SendMessageAsync(
            string userMessage,
            List<ConversationHistoryItem>? conversationHistory = null,
            string? systemPrompt = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📤 Sending message to Gemini AI...");

                // بناء الـ Request
                var request = BuildGeminiRequest(userMessage, conversationHistory, systemPrompt);

                // إرسال الـ Request
                // استخدم v1beta للـ Models الجديدة (1.5-flash, 1.5-pro)
                // استخدم v1 للـ Models القديمة (gemini-pro)
                var apiVersion = _modelName.Contains("1.5") ? "v1beta" : "v1";
                var apiUrl = $"https://generativelanguage.googleapis.com/{apiVersion}/models/{_modelName}:generateContent?key={_apiKey}";
                
                var response = await _httpClient.PostAsJsonAsync(apiUrl, request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();

                stopwatch.Stop();

                if (result?.Candidates == null || !result.Candidates.Any())
                {
                    _logger.LogWarning("⚠️ No response from Gemini AI");
                    return new GeminiResponse
                    {
                        HasError = true,
                        ErrorMessage = "لم يتم الحصول على رد من الـ AI",
                        ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                    };
                }

                var reply = result.Candidates[0]?.Content?.Parts?[0]?.Text ?? string.Empty;
                var tokenCount = result.UsageMetadata?.TotalTokenCount ?? 0;

                _logger.LogInformation("✅ Received response from Gemini AI in {Ms}ms", stopwatch.ElapsedMilliseconds);

                return new GeminiResponse
                {
                    Reply = reply,
                    TokenCount = tokenCount,
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    HasError = false
                };
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ HTTP error while calling Gemini AI");
                
                return new GeminiResponse
                {
                    HasError = true,
                    ErrorMessage = "حدث خطأ في الاتصال بالـ AI",
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Unexpected error in GeminiAIService");
                
                return new GeminiResponse
                {
                    HasError = true,
                    ErrorMessage = "حدث خطأ غير متوقع",
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
        }

        public string GetSystemPrompt(string userRole)
        {
            return userRole.ToLower() switch
            {
                "patient" => @"أنت مساعد ذكي ومُطمئن لمنصة شُريان الطبية. دورك مساعدة المرضى بأسلوب دافئ وداعم.

📋 **أسلوب التعامل مع الشكاوى الصحية:**

1️⃣ **الطمأنينة أولاً** (جملة واحدة دافئة):
   - ""ربنا يشفيك ويعافيك 🤲""
   - ""متقلقش، إن شاء الله خير""
   - ""أتمنى تحس بتحسن قريب 💙""

2️⃣ **نصائح عامة مفيدة** (2-3 نصائح بسيطة):
   - مشروبات دافئة (ينسون، نعناع، زنجبيل)
   - راحة وهدوء
   - تمارين خفيفة أو تنفس عميق
   - أكل صحي خفيف
   - شرب مياه كتير
   
   ⚠️ **مهم:** لا تعطي تشخيص طبي أبداً!

3️⃣ **اقتراح التخصص المناسب**:
   - ""أنصحك تستشير دكتور [التخصص]""
   - أمثلة: باطنة، قلب، عظام، جهاز هضمي، إلخ

4️⃣ **مساعدة في البحث**:
   - ""تقدر تدور على دكتور [التخصص] من المنصة""
   - ""عاوز أساعدك تلاقي دكتور قريب منك؟""

📌 **مهامك الأخرى:**
- البحث عن أطباء حسب التخصص والموقع
- حجز المواعيد وإدارتها
- شرح كيفية استخدام المنصة
- الإجابة على أسئلة عامة

💬 **أسلوب الكلام:**
- عربي فصيح بسيط وواضح
- دافئ وودود ومُطمئن
- مختصر ومباشر (3-5 أسطر)
- استخدم إيموجي بسيط للتوضيح

✅ **مثال على رد مثالي:**
المريض: ""بطني بتوجعني جداً""
الرد: 
""ربنا يشفيك ويعافيك 🤲

للتخفيف من الألم، جرب:
• شرب مشروب دافئ (ينسون أو نعناع) 🍵
• راحة وتجنب الأكل الثقيل
• كمادات دافئة على البطن

أنصحك تستشير دكتور جهاز هضمي أو باطنة للاطمئنان. عاوز أساعدك تلاقي دكتور قريب منك؟ 🏥""",

                "doctor" => @"أنت مساعد ذكي لمنصة شُريان الطبية.
دورك مساعدة الأطباء في:
- إدارة المواعيد والجدول الزمني
- متابعة المرضى وسجلاتهم
- إدارة الملف الشخصي والعيادة
- الإحصائيات والتقارير
- الإجابة على الأسئلة حول استخدام المنصة

تحدث بالعربية بأسلوب احترافي ومباشر.",

                "laboratory" => @"أنت مساعد ذكي لمنصة شُريان الطبية.
دورك مساعدة المعامل في:
- إدارة طلبات التحاليل
- رفع نتائج التحاليل
- إدارة الملف الشخصي
- متابعة الإحصائيات

تحدث بالعربية بأسلوب احترافي.",

                "pharmacy" => @"أنت مساعد ذكي لمنصة شُريان الطبية.
دورك مساعدة الصيدليات في:
- إدارة طلبات الأدوية
- صرف الروشتات
- إدارة المخزون
- متابعة الإحصائيات

تحدث بالعربية بأسلوب احترافي.",

                _ => @"أنت مساعد ذكي لمنصة شُريان الطبية.
دورك مساعدة المستخدمين في استخدام المنصة والإجابة على استفساراتهم.
تحدث بالعربية بأسلوب ودود ومهني."
            };
        }

        private object BuildGeminiRequest(
            string userMessage,
            List<ConversationHistoryItem>? conversationHistory,
            string? systemPrompt)
        {
            var contents = new List<object>();

            // أضف الـ System Prompt كأول رسالة
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = $"[System Instructions]\n{systemPrompt}" } }
                });
                contents.Add(new
                {
                    role = "model",
                    parts = new[] { new { text = "فهمت. سأساعدك حسب هذه التعليمات." } }
                });
            }

            // أضف تاريخ المحادثة
            if (conversationHistory != null && conversationHistory.Any())
            {
                foreach (var item in conversationHistory.TakeLast(10)) // آخر 10 رسائل فقط
                {
                    contents.Add(new
                    {
                        role = item.Role == "user" ? "user" : "model",
                        parts = new[] { new { text = item.Content } }
                    });
                }
            }

            // أضف الرسالة الحالية
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            return new
            {
                contents,
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 1024,
                }
            };
        }

        #region Gemini API Response Models
        
        private class GeminiApiResponse
        {
            public List<Candidate>? Candidates { get; set; }
            public UsageMetadata? UsageMetadata { get; set; }
        }

        private class Candidate
        {
            public Content? Content { get; set; }
        }

        private class Content
        {
            public List<Part>? Parts { get; set; }
        }

        private class Part
        {
            public string? Text { get; set; }
        }

        private class UsageMetadata
        {
            public int PromptTokenCount { get; set; }
            public int CandidatesTokenCount { get; set; }
            public int TotalTokenCount { get; set; }
        }

        #endregion
    }
}
