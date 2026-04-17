using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shuryan.Application.DTOs.Requests.Chat
{
    /// <summary>
    /// Request لإرسال رسالة للـ AI Bot
    /// </summary>
    public class SendMessageRequest
    {
        /// <summary>
        /// محتوى الرسالة من المستخدم
        /// </summary>
        [Required(ErrorMessage = "الرسالة مطلوبة")]
        [StringLength(2000, ErrorMessage = "الرسالة لا يمكن أن تتجاوز 2000 حرف")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Context إضافي عن حالة المستخدم الحالية
        /// </summary>
        public MessageContextDto? Context { get; set; }
    }

    /// <summary>
    /// Context الرسالة - معلومات عن الصفحة الحالية والحالة
    /// </summary>
    public class MessageContextDto
    {
        /// <summary>
        /// الصفحة الحالية (مثلاً: "search-doctors", "appointments")
        /// </summary>
        public string? CurrentPage { get; set; }

        /// <summary>
        /// معرف الدكتور إذا كان المستخدم على صفحة دكتور معين
        /// </summary>
        public Guid? DoctorId { get; set; }

        /// <summary>
        /// معرف الموعد إذا كان المستخدم بيشوف موعد معين
        /// </summary>
        public Guid? AppointmentId { get; set; }

        /// <summary>
        /// التخصص المختار في البحث
        /// </summary>
        public string? Specialty { get; set; }

        /// <summary>
        /// أي معلومات إضافية
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}
