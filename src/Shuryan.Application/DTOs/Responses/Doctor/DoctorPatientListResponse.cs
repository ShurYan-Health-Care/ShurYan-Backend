using System;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// معلومات مختصرة عن مريض معين للدكتور
    /// </summary>
    public class DoctorPatientListItemResponse
    {
        public Guid PatientId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public DateTime? LastSessionDate { get; set; }
        public double? PatientRating { get; set; } // تقييم المريض للدكتور (متوسط كل التقييمات)
    }
}
