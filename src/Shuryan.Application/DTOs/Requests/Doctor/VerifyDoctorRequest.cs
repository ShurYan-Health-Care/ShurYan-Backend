using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class VerifyDoctorRequest
    {
        [Required(ErrorMessage = "Verification status is required")]
        public bool IsVerified { get; set; }

        public Guid? VerifierId { get; set; }

        [StringLength(1000, ErrorMessage = "Verification notes cannot exceed 1000 characters")]
        public string? VerificationNotes { get; set; }
    }
}

