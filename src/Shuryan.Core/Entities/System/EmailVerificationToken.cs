using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.System
{
    public class EmailVerification
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }
        public int AttemptCount { get; set; } = 0;

        [MaxLength(50)]
        public string? RequestedFromIp { get; set; }

        [Required]
        public VerificationTypes VerificationType { get; set; } = VerificationTypes.EmailVerification;

        // Navigation Property
        public virtual User User { get; set; } = null!;

        public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt && AttemptCount < 5;
    }
}