using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Payment
{
    public class RefundPaymentRequest
    {
        [Required(ErrorMessage = "معرف الدفع مطلوب")]
        public Guid PaymentId { get; set; }

        [Required(ErrorMessage = "مبلغ الاسترجاع مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "سبب الاسترجاع مطلوب")]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
