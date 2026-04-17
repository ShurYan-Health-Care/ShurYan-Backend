using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums.Chat;
using System;
using System.Text.Json;

namespace Shuryan.Core.Entities.System
{
    public class ConversationMessage : AuditableEntity
    {
        public Guid ConversationId { get; set; }
        public MessageRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? SuggestionsJson { get; set; }
        public string? ActionsJson { get; set; }
        public string? ContextJson { get; set; }
        public int? TokenCount { get; set; }
        public int? ResponseTimeMs { get; set; }
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
