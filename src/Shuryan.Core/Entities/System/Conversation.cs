using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;

namespace Shuryan.Core.Entities.System
{
    /// <summary>
    /// محادثة مع الـ AI Bot
    /// كل User عنده محادثات كتير، وكل محادثة فيها رسائل
    /// </summary>
    public class Conversation : AuditableEntity
    {
        /// <summary>
        /// User اللي بيتكلم مع الـ Bot
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// دور المستخدم (Patient, Doctor, Laboratory, Pharmacy)
        /// </summary>
        public UserRole UserRole { get; set; }

        /// <summary>
        /// عنوان المحادثة (اختياري)
        /// مثلاً: "حجز موعد مع دكتور قلب"
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// آخر رسالة في المحادثة (للعرض السريع)
        /// </summary>
        public string? LastMessage { get; set; }

        /// <summary>
        /// تاريخ آخر رسالة
        /// </summary>
        public DateTime? LastMessageAt { get; set; }

        /// <summary>
        /// هل المحادثة نشطة ولا متأرشفة
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// الرسائل الخاصة بالمحادثة دي
        /// </summary>
        public virtual ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
    }
}
