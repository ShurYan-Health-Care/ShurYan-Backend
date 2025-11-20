using Shuryan.Core.Enums.Pharmacy;
using System;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyOrderStatusUpdateResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public PharmacyOrderStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
