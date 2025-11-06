using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;

namespace Shuryan.Core.Entities.System
{
    public class Conversation : AuditableEntity
    {
        public Guid UserId { get; set; }
        public UserRole UserRole { get; set; }
        public string? Title { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
    }
}
