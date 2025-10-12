using Shuryan.Application.DTOs.Common.Address;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class CreatePharmacyRequest
    {
        [Required(ErrorMessage = "Pharmacy name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Pharmacy name must be between 3-200 characters")]
        public string Name { get; set; } = string.Empty;

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

