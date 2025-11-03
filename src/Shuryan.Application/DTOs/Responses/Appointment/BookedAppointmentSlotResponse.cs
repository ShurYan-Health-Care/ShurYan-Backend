using System;

namespace Shuryan.Application.DTOs.Responses.Appointment
{
    /// <summary>
    /// Response DTO للمواعيد المحجوزة في يوم معين
    /// </summary>
    public class BookedAppointmentSlotResponse
    {
        /// <summary>
        /// معرف الموعد المحجوز
        /// </summary>
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// الوقت المحجوز بصيغة 24-hour (HH:mm) مثل "09:00"
        /// </summary>
        public string Time { get; set; } = string.Empty;

        /// <summary>
        /// اسم المريض (اختياري)
        /// </summary>
        public string? PatientName { get; set; }

        /// <summary>
        /// نوع الاستشارة: 0=Regular, 1=ReExamination
        /// </summary>
        public int ConsultationType { get; set; }
    }
}
