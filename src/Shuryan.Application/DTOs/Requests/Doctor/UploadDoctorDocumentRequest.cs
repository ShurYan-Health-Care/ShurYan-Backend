using Microsoft.AspNetCore.Http;
using Shuryan.Core.Enums.Doctor;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UploadDoctorDocumentRequest
    {
        [Required(ErrorMessage = "Document file is required")]
        public IFormFile DocumentFile { get; set; } = null!;

        [Required(ErrorMessage = "Document type is required")]
        public DoctorDocumentType Type { get; set; }
    }
}
