using Shuryan.Core.Enums.Appointments;
using System;

namespace Shuryan.Application.DTOs.Responses.Appointment
{
    /// <summary>
    /// Response DTO مبسط لعرض مواعيد الدكتور
    /// </summary>
    public class DoctorAppointmentResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? PatientPhoneNumber { get; set; }
        public string AppointmentDate { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty; // HH:mm:ss
        public int Duration { get; set; } // بالدقائق
        public string AppointmentType { get; set; } = string.Empty; // "regular" or "followup"
        public AppointmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } // تاريخ إنشاء الجلسة
        public string? Notes { get; set; }
        public decimal Price { get; set; }
    }
}
