using System;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// Response DTO لعرض معلومات المرضى التابعين للدكتور
    /// </summary>
    public class DoctorPatientResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int TotalSessions { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public string? Address { get; set; }
        public decimal? Rating { get; set; }
    }
}
