using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class UpdatePharmacyProfileImageRequest
    {
        [Required(ErrorMessage = "Profile image file is required")]
        public IFormFile ProfileImage { get; set; } = null!;
    }
}
