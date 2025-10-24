using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UpdateProfileImageRequest
    {
        [Required(ErrorMessage = "Image URL is required")]
        [Url(ErrorMessage = "Invalid image URL")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
