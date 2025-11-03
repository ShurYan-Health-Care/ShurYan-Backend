using System;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    /// <summary>
    /// Request DTO لحجز موعد جديد
    /// </summary>
    public class BookAppointmentRequest
    {
        /// <summary>
        /// معرف الدكتور
        /// </summary>
        public Guid DoctorId { get; set; }

        /// <summary>
        /// تاريخ الموعد بصيغة YYYY-MM-DD
        /// </summary>
        public string AppointmentDate { get; set; } = string.Empty;

        /// <summary>
        /// وقت الموعد بصيغة 24-hour (HH:mm) مثل "09:00"
        /// </summary>
        public string AppointmentTime { get; set; } = string.Empty;

        /// <summary>
        /// نوع الاستشارة: 0=Regular, 1=ReExamination
        /// </summary>
        public int ConsultationType { get; set; }
    }
}
