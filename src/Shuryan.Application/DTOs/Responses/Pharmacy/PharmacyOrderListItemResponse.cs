using Shuryan.Core.Enums.Pharmacy;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyOrderListItemResponse
    {
        public Guid OrderId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalCost { get; set; }
        public PharmacyOrderStatus Status { get; set; }
    }
}
