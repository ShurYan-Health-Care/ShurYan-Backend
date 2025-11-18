using System;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorPatientDto
    {
        public Guid PatientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImageUrl { get; set; }
        public string City { get; set; }
        public string Governorate { get; set; }
        public int TotalSessions { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public decimal? Rating { get; set; }
    }
}
