using Shuryan.Core.Enums.Pharmacy;
using System;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyOrderResponse
    {
        public Guid OrderId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public PharmacyOrderStatus PharmacyOrderStatus { get; set; }
        public DateTime ReceivedAt { get; set; }
        public PharmacyOrderPrescriptionResponse? Prescription { get; set; }
    }
}
