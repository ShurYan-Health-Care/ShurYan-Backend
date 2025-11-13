using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Enums.Payment;

namespace Shuryan.Core.Entities.External.Payments
{
    public class PaymentTransaction
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid PaymentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TransactionType { get; set; } = string.Empty; 

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [MaxLength(200)]
        public string? ProviderTransactionId { get; set; }

        [MaxLength(1000)]
        public string? ProviderResponse { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        [MaxLength(50)]
        public string? ErrorCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        [MaxLength(1000)]
        public string? Metadata { get; set; }

        // Navigation Property
        public virtual Payment Payment { get; set; } = null!;
    }
}
