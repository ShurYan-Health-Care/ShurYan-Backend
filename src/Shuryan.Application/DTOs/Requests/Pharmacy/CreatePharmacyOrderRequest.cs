using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class CreatePharmacyOrderRequest
    {
        [Required(ErrorMessage = "Pharmacy ID is required")]
        public Guid PharmacyId { get; set; }

        public Guid? PrescriptionId { get; set; }

        [Required(ErrorMessage = "Delivery type is required")]
        public OrderDeliveryType DeliveryType { get; set; }

        [Required(ErrorMessage = "Delivery person phone is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10-20 characters")]
        public string DeliveryPersonPhone { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Delivery person name must be between 2-100 characters")]
        public string? DeliveryPersonName { get; set; }

        [StringLength(500, ErrorMessage = "Delivery notes cannot exceed 500 characters")]
        public string? DeliveryNotes { get; set; }
    }
}
