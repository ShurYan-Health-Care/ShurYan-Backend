using System;

namespace Shuryan.Application.DTOs.Responses.Session
{
    /// <summary>
    /// Response لبيانات الجلسة
    /// </summary>
    public class SessionResponse
    {
        public Guid SessionId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientPhone { get; set; }
        
        // 🆕 معلومات إضافية عن المريض
        public int? PatientAge { get; set; }
        public string? PatientProfileImageUrl { get; set; }
        
        // معلومات الجلسة
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Duration { get; set; }
        public int SessionType { get; set; }
        public string Status { get; set; } = null!;
        
        // 🆕 معلومات الموعد
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
    }
}
