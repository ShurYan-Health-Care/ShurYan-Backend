using System;
using System.Collections.Generic;
using Shuryan.Core.Enums.Payment;

namespace Shuryan.Application.DTOs.Responses.Payment
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string OrderType { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public PaymentProvider? Provider { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderTransactionId { get; set; }
        public decimal? RefundedAmount { get; set; }
        public DateTime? RefundedAt { get; set; }
        public string? RefundReason { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public string? FailureReason { get; set; }
        public List<PaymentTransactionResponse> Transactions { get; set; } = new();
    }
}
