using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    /// <summary>
    /// Request to accept prescription for delivery by pharmacy
    /// </summary>
    public class AcceptDeliveryRequest
    {
        [Required]
        public Guid PharmacyId { get; set; }

        [Range(15, 1440, ErrorMessage = "Delivery time must be between 15 minutes and 24 hours")]
        public int EstimatedDeliveryMinutes { get; set; }

        [Required]
        [StringLength(500)]
        public string DeliveryAddress { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal DeliveryFee { get; set; }

        [StringLength(500)]
        public string? DeliveryNotes { get; set; }
    }
}
