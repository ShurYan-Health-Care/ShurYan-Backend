using System;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyOrderResponseResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PrescriptionNumber { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public bool DeliveryAvailable { get; set; }
        public DateTime RespondedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
