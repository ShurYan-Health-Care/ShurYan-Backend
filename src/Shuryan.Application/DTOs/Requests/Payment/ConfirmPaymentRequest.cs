using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Payment
{
    public class ConfirmPaymentRequest
    {
        [Required(ErrorMessage = "معرف الدفع مطلوب")]
        public Guid PaymentId { get; set; }

        [MaxLength(200)]
        public string? ProviderTransactionId { get; set; }

        [MaxLength(500)]
        public string? ProviderResponse { get; set; }
    }
}
