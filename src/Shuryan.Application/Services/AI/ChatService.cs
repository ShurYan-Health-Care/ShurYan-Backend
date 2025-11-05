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

                // 1. جيب أو اعمل محادثة
                Conversation conversation;
                if (request.ConversationId.HasValue)
                {
                    // محادثة موجودة
                    conversation = await _unitOfWork.Conversations
                        .GetConversationWithMessagesAsync(request.ConversationId.Value);

                    if (conversation == null || conversation.UserId != userId)
                    {
                        _logger.LogWarning("⚠️ Conversation not found or unauthorized");
                        return null;
                    }
                }
                else
                {
                    // محادثة جديدة
                    conversation = new Conversation
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        UserRole = Enum.Parse<UserRole>(userRole, true),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Conversations.AddAsync(conversation);
                    
                    // احفظ الـ Conversation الأول قبل ما تضيف Messages
                    await _unitOfWork.SaveChangesAsync();
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

                _logger.LogInformation("✅ Message processed successfully");

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

        public async Task<ConversationResponse?> GetConversationAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _unitOfWork.Conversations
                    .GetConversationWithMessagesAsync(conversationId);

                if (conversation == null || conversation.UserId != userId)
                    return null;

                return new ConversationResponse
                {
                    Id = conversation.Id,
                    Title = conversation.Title,
                    LastMessage = conversation.LastMessage,
                    LastMessageAt = conversation.LastMessageAt,
                    IsActive = conversation.IsActive,
                    CreatedAt = conversation.CreatedAt,
                    Messages = conversation.Messages?
                        .OrderBy(m => m.CreatedAt)
                        .Select(m => new ConversationMessageDto
                        {
                            Id = m.Id,
                            Role = m.Role.ToString().ToLower(),
                            Content = m.Content,
                            Suggestions = m.GetSuggestions()?.ToList(),
                            Actions = !string.IsNullOrEmpty(m.ActionsJson)
                                ? JsonSerializer.Deserialize<List<ChatActionDto>>(m.ActionsJson)
                                : null,
                            CreatedAt = m.CreatedAt
                        })
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GetConversationAsync");
                return null;
            }
        }

        public async Task<IEnumerable<ConversationListItemResponse>> GetUserConversationsAsync(
            Guid userId, 
            bool activeOnly = true)
        {
            try
            {
                var conversations = await _unitOfWork.Conversations
                    .GetUserConversationsAsync(userId, activeOnly);

                return conversations.Select(c => new ConversationListItemResponse
                {
                    Id = c.Id,
                    Title = c.Title,
                    LastMessage = c.LastMessage,
                    LastMessageAt = c.LastMessageAt,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    MessageCount = c.Messages?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GetUserConversationsAsync");
                return Enumerable.Empty<ConversationListItemResponse>();
            }
        }

        public async Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _unitOfWork.Conversations.GetByIdAsync(conversationId);
                
                if (conversation == null || conversation.UserId != userId)
                    return false;

                // امسح كل الرسائل الأول
                await _unitOfWork.ConversationMessages.DeleteConversationMessagesAsync(conversationId);

                // امسح المحادثة
                _unitOfWork.Conversations.Delete(conversation);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("🗑️ Conversation {ConversationId} deleted", conversationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in DeleteConversationAsync");
                return false;
            }
        }

        public async Task<bool> ArchiveConversationAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _unitOfWork.Conversations.GetByIdAsync(conversationId);
                
                if (conversation == null || conversation.UserId != userId)
                    return false;

                await _unitOfWork.Conversations.ArchiveConversationAsync(conversationId);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("📦 Conversation {ConversationId} archived", conversationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in ArchiveConversationAsync");
                return false;
            }
        }

        public async Task<bool> ClearConversationMessagesAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _unitOfWork.Conversations.GetByIdAsync(conversationId);
                
                if (conversation == null || conversation.UserId != userId)
                    return false;

                await _unitOfWork.ConversationMessages.DeleteConversationMessagesAsync(conversationId);

                // حدّث المحادثة
                conversation.LastMessage = null;
                conversation.LastMessageAt = null;
                conversation.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Conversations.Update(conversation);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("🧹 Conversation {ConversationId} messages cleared", conversationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in ClearConversationMessagesAsync");
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
