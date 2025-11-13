using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Chat;
using Shuryan.Application.DTOs.Responses.Chat;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums.Chat;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shuryan.Application.Services.AI
{
    /// <summary>
    /// Service للتعامل مع المحادثات والـ AI Bot
    /// بيربط بين الـ Gemini AI والـ Database
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeminiAIService _geminiAIService;
        private readonly IMapper _mapper;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IUnitOfWork unitOfWork,
            IGeminiAIService geminiAIService,
            IMapper mapper,
            ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _geminiAIService = geminiAIService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ChatMessageResponse?> SendMessageAsync(
            Guid userId,
            string userRole,
            SendMessageRequest request)
        {
            try
            {
                _logger.LogInformation("💬 User {UserId} sending message", userId);

                // 1. جيب أو اعمل محادثة واحدة للمستخدم
                var conversation = await _unitOfWork.Conversations
                    .GetUserActiveConversationAsync(userId);

                if (conversation == null)
                {
                    // اعمل محادثة جديدة
                    conversation = new Conversation
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        UserRole = Enum.Parse<UserRole>(userRole, true),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Conversations.AddAsync(conversation);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    // لو المحادثة موجودة، جيب آخر 10 رسائل للـ context
                    conversation = await _unitOfWork.Conversations
                        .GetConversationWithMessagesAsync(conversation.Id);
                }

                // 2. احفظ رسالة المستخدم
                var userMessage = new ConversationMessage
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    Role = MessageRole.User,
                    Content = request.Message,
                    ContextJson = request.Context != null 
                        ? JsonSerializer.Serialize(request.Context) 
                        : null,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ConversationMessages.AddAsync(userMessage);

                // 3. جهز تاريخ المحادثة للـ AI
                var conversationHistory = conversation.Messages?
                    .OrderBy(m => m.CreatedAt)
                    .TakeLast(10)
                    .Select(m => new ConversationHistoryItem
                    {
                        Role = m.Role == MessageRole.User ? "user" : "assistant",
                        Content = m.Content
                    })
                    .ToList() ?? new List<ConversationHistoryItem>();

                // 4. اعمل Context Enhancement (إثراء السياق)
                var enrichedMessage = EnrichMessageWithContext(request.Message, request.Context);

                // 5. ابعت للـ Gemini AI
                var systemPrompt = _geminiAIService.GetSystemPrompt(userRole);
                var aiResponse = await _geminiAIService.SendMessageAsync(
                    enrichedMessage,
                    conversationHistory,
                    systemPrompt
                );

                if (aiResponse.HasError)
                {
                    _logger.LogError("❌ AI returned error: {Error}", aiResponse.ErrorMessage);
                    return null;
                }

                // 6. استخرج Suggestions و Actions من رد الـ AI
                var (suggestions, actions) = ExtractSuggestionsAndActions(
                    aiResponse.Reply, 
                    request.Context
                );

                // 7. احفظ رد الـ AI
                var assistantMessage = new ConversationMessage
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    Role = MessageRole.Assistant,
                    Content = aiResponse.Reply,
                    TokenCount = aiResponse.TokenCount,
                    ResponseTimeMs = aiResponse.ResponseTimeMs,
                    CreatedAt = DateTime.UtcNow
                };

                assistantMessage.SetSuggestions(suggestions.ToArray());
                
                if (actions.Any())
                {
                    assistantMessage.ActionsJson = JsonSerializer.Serialize(actions);
                }

                await _unitOfWork.ConversationMessages.AddAsync(assistantMessage);

                // 8. حدّث المحادثة
                conversation.LastMessage = aiResponse.Reply.Length > 100 
                    ? aiResponse.Reply.Substring(0, 100) + "..." 
                    : aiResponse.Reply;
                conversation.LastMessageAt = DateTime.UtcNow;
                conversation.UpdatedAt = DateTime.UtcNow;

                // عنوان المحادثة (أول 50 حرف من أول رسالة)
                if (string.IsNullOrEmpty(conversation.Title))
                {
                    conversation.Title = request.Message.Length > 50
                        ? request.Message.Substring(0, 50) + "..."
                        : request.Message;
                }

                _unitOfWork.Conversations.Update(conversation);

                // 9. احفظ كل حاجة
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Message processed successfully");

                // 10. ارجع الـ Response
                return new ChatMessageResponse
                {
                    ConversationId = conversation.Id,
                    MessageId = assistantMessage.Id,
                    Reply = aiResponse.Reply,
                    Suggestions = suggestions,
                    Actions = actions,
                    Timestamp = assistantMessage.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in SendMessageAsync");
                return null;
            }
        }

        public async Task<ChatHistoryResponse?> GetChatHistoryAsync(
            Guid userId,
            int pageNumber = 1,
            int pageSize = 50)
        {
            try
            {
                // جيب محادثة المستخدم
                var conversation = await _unitOfWork.Conversations
                    .GetUserActiveConversationAsync(userId);

                if (conversation == null)
                    return null;

                // جيب عدد الرسائل الكلي
                var totalMessages = await _unitOfWork.ConversationMessages
                    .GetConversationMessageCountAsync(conversation.Id);

                if (totalMessages == 0)
                {
                    return new ChatHistoryResponse
                    {
                        ConversationId = conversation.Id,
                        Messages = new List<ChatMessageDto>(),
                        Pagination = new PaginationInfo
                        {
                            CurrentPage = 1,
                            PageSize = pageSize,
                            TotalMessages = 0,
                            TotalPages = 0
                        },
                        HasMore = false
                    };
                }

                // احسب الـ Pagination
                var totalPages = (int)Math.Ceiling(totalMessages / (double)pageSize);
                var skip = (pageNumber - 1) * pageSize;

                // جيب الرسائل مع Pagination (من الأحدث للأقدم)
                var messages = await _unitOfWork.ConversationMessages
                    .GetConversationMessagesPagedAsync(conversation.Id, skip, pageSize);

                var messageDtos = messages
                    .Select(m => new ChatMessageDto
                    {
                        MessageId = m.Id,
                        Role = m.Role.ToString().ToLower(),
                        Content = m.Content,
                        Suggestions = m.GetSuggestions()?.ToList(),
                        Actions = !string.IsNullOrEmpty(m.ActionsJson)
                            ? JsonSerializer.Deserialize<List<ChatActionDto>>(m.ActionsJson)
                            : null,
                        Timestamp = m.CreatedAt
                    })
                    .ToList();

                return new ChatHistoryResponse
                {
                    ConversationId = conversation.Id,
                    Messages = messageDtos,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalMessages = totalMessages,
                        TotalPages = totalPages
                    },
                    HasMore = pageNumber < totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GetChatHistoryAsync");
                return null;
            }
        }

        public async Task<bool> ClearUserChatAsync(Guid userId)
        {
            try
            {
                var conversation = await _unitOfWork.Conversations
                    .GetUserActiveConversationAsync(userId);

                if (conversation == null)
                    return false;

                // امسح كل الرسائل
                await _unitOfWork.ConversationMessages
                    .DeleteConversationMessagesAsync(conversation.Id);

                // حدّث المحادثة
                conversation.LastMessage = null;
                conversation.LastMessageAt = null;
                conversation.Title = null;
                conversation.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Conversations.Update(conversation);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("🧹 User {UserId} chat cleared", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in ClearUserChatAsync");
                return false;
            }
        }

        #region Helper Methods

        /// <summary>
        /// إثراء الرسالة بالـ Context
        /// </summary>
        private string EnrichMessageWithContext(string message, MessageContextDto? context)
        {
            if (context == null)
                return message;

            var contextInfo = new List<string>();

            if (!string.IsNullOrEmpty(context.CurrentPage))
                contextInfo.Add($"الصفحة الحالية: {context.CurrentPage}");

            if (context.DoctorId.HasValue)
                contextInfo.Add($"معرف الدكتور: {context.DoctorId}");

            if (!string.IsNullOrEmpty(context.Specialty))
                contextInfo.Add($"التخصص: {context.Specialty}");

            if (!contextInfo.Any())
                return message;

            return $"{message}\n\n[Context: {string.Join(", ", contextInfo)}]";
        }

        /// <summary>
        /// استخراج Suggestions و Actions من رد الـ AI
        /// </summary>
        private (List<string> suggestions, List<ChatActionDto> actions) ExtractSuggestionsAndActions(
            string aiReply,
            MessageContextDto? context)
        {
            var suggestions = new List<string>();
            var actions = new List<ChatActionDto>();

            // Suggestions افتراضية حسب الـ Context
            if (context?.CurrentPage == "search-doctors")
            {
                suggestions.Add("شوف الدكاترة المتاحين");
                suggestions.Add("فلتر حسب التخصص");
                
                actions.Add(new ChatActionDto
                {
                    Type = "navigate",
                    Route = "/patient/search",
                    Label = "ابحث عن دكتور"
                });
            }
            else if (context?.DoctorId.HasValue == true)
            {
                suggestions.Add("احجز موعد");
                suggestions.Add("شوف المواعيد المتاحة");
                
                actions.Add(new ChatActionDto
                {
                    Type = "navigate",
                    Route = $"/patient/doctors/{context.DoctorId}",
                    Label = "شوف ملف الدكتور"
                });
            }
            else
            {
                // Suggestions عامة
                suggestions.Add("ابحث عن دكتور");
                suggestions.Add("شوف مواعيدي");
                suggestions.Add("كيف أستخدم المنصة؟");
            }

            return (suggestions, actions);
        }

        #endregion
    }
}
