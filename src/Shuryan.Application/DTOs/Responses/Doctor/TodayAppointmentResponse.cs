using System;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// Response DTO لموعد واحد من مواعيد اليوم
    /// </summary>
    public class TodayAppointmentResponse
    {
        /// <summary>
        /// معرف الموعد
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// معرف المريض
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// اسم المريض الكامل
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// رقم تليفون المريض
        /// </summary>
        public string? PatientPhoneNumber { get; set; }

        /// <summary>
        /// وقت الموعد بصيغة 24-hour (HH:mm) مثل "09:00"
        /// </summary>
        public string AppointmentTime { get; set; } = string.Empty;

        /// <summary>
        /// تاريخ الموعد بصيغة yyyy-MM-dd
        /// </summary>
        public string AppointmentDate { get; set; } = string.Empty;

        /// <summary>
        /// مدة الموعد بالدقائق
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// نوع الموعد: "regular" أو "followup"
        /// </summary>
        public string AppointmentType { get; set; } = string.Empty;

        /// <summary>
        /// حالة الموعد: "pending", "completed", "cancelled"
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// ملاحظات على الموعد (اختياري)
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// سعر الكشف
        /// </summary>
        public decimal Price { get; set; }
    }
}
