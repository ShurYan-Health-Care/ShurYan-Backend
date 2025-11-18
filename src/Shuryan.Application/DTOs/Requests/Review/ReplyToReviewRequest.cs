using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Review
{
    public class ReplyToReviewRequest
    {
        [Required(ErrorMessage = "Response required")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "The response must be between 3 and 300 characters.")]
        public string Reply { get; set; } = string.Empty;
    }
}

