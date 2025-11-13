using System;

namespace Shuryan.Application.DTOs.Responses.Patient
{
    public class SendPrescriptionResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string PharmacyName { get; set; } = string.Empty;
        public string PrescriptionNumber { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
