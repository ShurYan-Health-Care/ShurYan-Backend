using System;
using System.ComponentModel.DataAnnotations;
using Shuryan.Core.Enums.Payment;

namespace Shuryan.Application.DTOs.Requests.Payment
{
    public class InitiatePaymentRequest
    {
        [Required(ErrorMessage = "نوع الطلب مطلوب")]
        public string OrderType { get; set; } = string.Empty; // PharmacyOrder, LabOrder, ConsultationBooking

        [Required(ErrorMessage = "معرف الطلب مطلوب")]
        public Guid OrderId { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "طريقة الدفع مطلوبة")]
        public PaymentMethod PaymentMethod { get; set; }

        public PaymentProvider? Provider { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? ReturnUrl { get; set; }
    }
}
