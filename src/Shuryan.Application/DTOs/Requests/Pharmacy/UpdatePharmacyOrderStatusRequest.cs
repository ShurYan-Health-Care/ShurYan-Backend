using Shuryan.Core.Enums.Pharmacy;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class UpdatePharmacyOrderStatusRequest
    {
        [Required(ErrorMessage = "الحالة الجديدة مطلوبة")]
        public PharmacyOrderStatus NewStatus { get; set; }

        // Optional metadata for delivery flows
        public DateTime? EstimatedDeliveryTime { get; set; }

        [MaxLength(100)]
        public string? DeliveryPersonName { get; set; }

        [MaxLength(25)]
        public string? DeliveryPersonPhone { get; set; }

        [MaxLength(500)]
        public string? DeliveryNotes { get; set; }

        public DateTime? ActualDeliveryTime { get; set; }
    }
}
