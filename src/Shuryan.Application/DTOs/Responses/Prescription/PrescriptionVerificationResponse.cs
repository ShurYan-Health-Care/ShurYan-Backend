using System;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Response for prescription verification (used by pharmacies)
    /// </summary>
    public class PrescriptionVerificationResponse
    {
        public bool IsValid { get; set; }
        public string VerificationStatus { get; set; } = null!; // Valid, Expired, Cancelled, AlreadyDispensed, NotFound
        public PrescriptionResponse? Prescription { get; set; }
        
        // Verification details
        public DateTime VerifiedAt { get; set; }
        public string? VerificationMessage { get; set; }
        public bool CanBeDispensed { get; set; }
        public string? ReasonCannotBeDispensed { get; set; }

        // Security
        public string DigitalSignatureStatus { get; set; } = null!; // Valid, Invalid, NotPresent
        public bool DoctorLicenseActive { get; set; }
        public bool PatientEligible { get; set; }
    }
}
