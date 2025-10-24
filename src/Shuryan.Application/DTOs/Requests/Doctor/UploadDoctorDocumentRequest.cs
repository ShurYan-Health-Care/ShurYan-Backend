using Shuryan.Core.Enums.Doctor;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UploadDoctorDocumentRequest
    {
        [Required(ErrorMessage = "Document URL is required")]
        [Url(ErrorMessage = "Invalid document URL")]
        [StringLength(500, ErrorMessage = "Document URL cannot exceed 500 characters")]
        public string DocumentUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document type is required")]
        public DoctorDocumentType Type { get; set; }
    }
}
