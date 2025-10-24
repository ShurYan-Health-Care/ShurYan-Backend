using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class RejectDocumentRequest
    {
        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
