using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class RejectVerificationRequest
    {
        [Required(ErrorMessage = "Verifier ID is required")]
        public Guid VerifierId { get; set; }

        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Rejection reason must be between 10-1000 characters")]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
