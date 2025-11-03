using System;

namespace Shuryan.Application.DTOs.Responses.Appointment
{
    /// <summary>
    /// Response DTO بعد حجز موعد بنجاح
    /// </summary>
    public class BookedAppointmentResponse
    {
        /// <summary>
        /// معرف الموعد المحجوز
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// معرف الدكتور
        /// </summary>
        public Guid DoctorId { get; set; }

        /// <summary>
        /// معرف المريض
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// تاريخ الموعد بصيغة YYYY-MM-DD
        /// </summary>
        public string AppointmentDate { get; set; } = string.Empty;

        /// <summary>
        /// وقت الموعد بصيغة 24-hour (HH:mm)
        /// </summary>
        public string AppointmentTime { get; set; } = string.Empty;

        /// <summary>
        /// نوع الاستشارة: 0=Regular, 1=ReExamination
        /// </summary>
        public int ConsultationType { get; set; }

        /// <summary>
        /// حالة الموعد (Pending, Confirmed, etc.)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// وقت الإنشاء
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
