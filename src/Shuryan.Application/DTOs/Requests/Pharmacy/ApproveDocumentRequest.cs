using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class ApproveDocumentRequest
    {
        [Required(ErrorMessage = "Verifier ID is required")]
        public Guid VerifierId { get; set; }

        public string? Notes { get; set; }
    }
}
