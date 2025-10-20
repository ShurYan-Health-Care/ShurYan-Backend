using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Shared.Configurations
{
    /// <summary>
    /// SMTP Email Configuration for sending OTP and verification emails
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// SMTP Server Host (e.g., smtp.gmail.com)
        /// </summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// SMTP Port (typically 587 for TLS, 465 for SSL)
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// Enable SSL/TLS encryption
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// SMTP Username (email address)
        /// </summary>
        public string SmtpUsername { get; set; } = string.Empty;

        /// <summary>
        /// SMTP Password or App-Specific Password
        /// </summary>
        public string SmtpPassword { get; set; } = string.Empty;

        /// <summary>
        /// Display name for sender
        /// </summary>
        public string FromName { get; set; } = "Shuryan Healthcare";

        /// <summary>
        /// Sender email address
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;

        /// <summary>
        /// Email verification OTP expiration in minutes (default: 10)
        /// </summary>
        public int VerificationOtpExpirationMinutes { get; set; } = 10;

        /// <summary>
        /// Password reset OTP expiration in minutes (default: 15)
        /// </summary>
        public int PasswordResetOtpExpirationMinutes { get; set; } = 15;

        /// <summary>
        /// Base URL for email links (e.g., https://yourdomain.com)
        /// </summary>
        public string ApplicationBaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// OTP length (default: 6)
        /// </summary>
        public int OtpLength { get; set; } = 6;

        /// <summary>
        /// Maximum OTP resend attempts per hour
        /// </summary>
        public int MaxOtpResendAttemptsPerHour { get; set; } = 5;
    }
}