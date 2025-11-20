using Shuryan.Core.Enums.Pharmacy;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyOrderDetailResponse
    {
        public Guid OrderId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public PharmacyOrderStatus Status { get; set; }
        public List<OrderMedicationDetailResponse> Medications { get; set; } = new();
        public decimal DeliveryFee { get; set; }
        public decimal TotalCost { get; set; }
        public OrderDeliveryInfoResponse DeliveryInfo { get; set; } = new();
    }
}
