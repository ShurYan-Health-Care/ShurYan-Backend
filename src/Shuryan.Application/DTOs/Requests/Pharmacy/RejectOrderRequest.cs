using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class RejectOrderRequest
    {
        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Rejection reason must be between 10-500 characters")]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
