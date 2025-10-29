using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class UploadClinicImageRequest
    {
        [Required(ErrorMessage = "الصورة مطلوبة")]
        public IFormFile Image { get; set; } = null!;

        [Required(ErrorMessage = "ترتيب الصورة مطلوب")]
        [Range(0, 5, ErrorMessage = "ترتيب الصورة يجب أن يكون بين 0 و 5")]
        public int Order { get; set; }
    }
}
