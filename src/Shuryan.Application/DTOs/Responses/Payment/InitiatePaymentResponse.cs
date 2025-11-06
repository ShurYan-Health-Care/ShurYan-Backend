using System;

namespace Shuryan.Application.DTOs.Responses.Payment
{
    public class InitiatePaymentResponse
    {
        public Guid PaymentId { get; set; }
        public string? PaymentUrl { get; set; }
        public string? QrCode { get; set; }
        public string? ReferenceNumber { get; set; }
        public bool RequiresRedirect { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
