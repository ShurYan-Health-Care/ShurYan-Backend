using Shuryan.Core.Enums.Doctor;
using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// Response DTO for doctor's professional information
    /// المعلومات المهنية للدكتور (التخصص، الخبرة، المستندات)
    /// </summary>
    public class DoctorProfessionalInfoResponse
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        
        // Professional Details
        public MedicalSpecialty MedicalSpecialty { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        
        // Documents
        public IEnumerable<DoctorDocumentResponse> Documents { get; set; } = new List<DoctorDocumentResponse>();
        public int TotalDocuments { get; set; }
        public int ApprovedDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public int RejectedDocuments { get; set; }
    }
}
