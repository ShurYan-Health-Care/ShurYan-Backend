using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    /// <summary>
    /// Request to cancel a prescription
    /// </summary>
    public class CancelPrescriptionRequest
    {
        [Required(ErrorMessage = "Cancellation reason is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 500 characters")]
        public string Reason { get; set; } = null!;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
