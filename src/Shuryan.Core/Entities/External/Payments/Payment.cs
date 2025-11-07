using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Payment;

namespace Shuryan.Core.Entities.External.Payments
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderType { get; set; } = string.Empty; // PharmacyOrder, LabOrder, ConsultationBooking

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "EGP";

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public PaymentProvider? Provider { get; set; }

        [MaxLength(200)]
        public string? ProviderTransactionId { get; set; }

        [MaxLength(500)]
        public string? ProviderResponse { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RefundedAmount { get; set; }

        public DateTime? RefundedAt { get; set; }

        [MaxLength(500)]
        public string? RefundReason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public DateTime? FailedAt { get; set; }

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    }
}
