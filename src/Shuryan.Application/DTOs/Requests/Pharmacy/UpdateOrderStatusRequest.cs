using Shuryan.Core.Enums.Pharmacy;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class UpdateOrderStatusRequest
    {
        [Required(ErrorMessage = "Status is required")]
        public PharmacyOrderStatus Status { get; set; }

        public string? Notes { get; set; }

        public DateTime? EstimatedDeliveryTime { get; set; }

        public string? DeliveryPersonName { get; set; }

        public string? DeliveryPersonPhone { get; set; }
    }
}
