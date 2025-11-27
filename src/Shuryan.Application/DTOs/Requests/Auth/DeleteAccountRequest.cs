using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Auth
{
    public class DeleteAccountRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirmation is required")]
        public string ConfirmationText { get; set; } = string.Empty;
    }
}
