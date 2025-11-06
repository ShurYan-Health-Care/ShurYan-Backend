using System;
using Shuryan.Core.Enums.Payment;

namespace Shuryan.Application.DTOs.Responses.Payment
{
    public class PaymentTransactionResponse
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? ProviderTransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
