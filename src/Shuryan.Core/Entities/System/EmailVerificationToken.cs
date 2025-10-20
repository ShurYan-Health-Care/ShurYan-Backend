using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Entities.System
{
    /// <summary>
    /// Email verification OTP tracking
    /// </summary>
    public class EmailVerification
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// User ID this verification belongs to
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Email address being verified
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 6-digit OTP code
        /// </summary>
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = string.Empty;

        /// <summary>
        /// OTP creation timestamp
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// OTP expiration timestamp
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Whether OTP has been used
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// When OTP was verified
        /// </summary>
        public DateTime? VerifiedAt { get; set; }

        /// <summary>
        /// Number of verification attempts
        /// </summary>
        public int AttemptCount { get; set; } = 0;

        /// <summary>
        /// IP address that requested OTP
        /// </summary>
        [MaxLength(50)]
        public string? RequestedFromIp { get; set; }

        /// <summary>
        /// Verification type (EmailVerification, PasswordReset, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string VerificationType { get; set; } = "EmailVerification";

        // Navigation Property
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Check if OTP is valid
        /// </summary>
        public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt && AttemptCount < 5;
    }

    /// <summary>
    /// Verification types enum
    /// </summary>
    public static class VerificationTypes
    {
        public const string EmailVerification = "EmailVerification";
        public const string PasswordReset = "PasswordReset";
    }
}