using System;

namespace Shuryan.Core.DTOs
{
    /// <summary>
    /// DTO for optimized doctor-patient query projection
    /// Used to avoid N+1 queries and cartesian explosion
    /// </summary>
    public class DoctorPatientDto
    {
        public Guid PatientId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? City { get; set; }
        public string? Governorate { get; set; }
        public int TotalSessions { get; set; }
        public DateTime? LastVisitDate { get; set; }
    }
}
