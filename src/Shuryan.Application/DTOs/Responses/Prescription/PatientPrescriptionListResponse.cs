using System;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Response لعرض قائمة الروشتات في بروفايل المريض
    /// </summary>
    public class PatientPrescriptionListResponse
    {
        public Guid Id { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // حالة الروشتة
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        
        // معلومات الطبيب
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;
        public string? DoctorProfileImageUrl { get; set; }
        
        // معلومات الحجز
        public string AppointmentType { get; set; } = string.Empty; // "regular" or "followup"
        public Guid? AppointmentId { get; set; }
    }
}
