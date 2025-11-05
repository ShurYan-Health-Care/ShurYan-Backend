namespace Shuryan.Core.Enums.Chat
{
    /// <summary>
    /// دور المرسل في المحادثة
    /// </summary>
    public enum MessageRole
    {
        /// <summary>
        /// رسالة من المستخدم
        /// </summary>
        User = 0,

        /// <summary>
        /// رسالة من الـ AI Assistant
        /// </summary>
        Assistant = 1,

        /// <summary>
        /// رسالة نظام (System Prompt)
        /// </summary>
        System = 2
    }
}
