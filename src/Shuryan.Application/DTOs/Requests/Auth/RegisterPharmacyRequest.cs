using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Common.Address;

namespace Shuryan.Application.DTOs.Requests.Auth
{
    public class RegisterPharmacyRequest
    {
        [Required(ErrorMessage = "Pharmacy name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Pharmacy name must be between 3-200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10-20 characters")]
        public string? PhoneNumber { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [Phone(ErrorMessage = "Invalid WhatsApp number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "WhatsApp number must be between 10-20 characters")]
        public string? WhatsAppNumber { get; set; }

        [Url(ErrorMessage = "Invalid website URL format")]
        public string? Website { get; set; }

        public bool OffersDelivery { get; set; } = true;

        [Required(ErrorMessage = "Address is required")]
        public CreateAddressDto Address { get; set; } = null!;
    }
}
