using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UpdateProfileImageRequest
    {
        [Required(ErrorMessage = "Profile image file is required")]
        public IFormFile ProfileImage { get; set; } = null!;
    }
}
