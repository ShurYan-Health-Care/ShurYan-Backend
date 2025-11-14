using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Shared.Configurations
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromName { get; set; } = "Shuryan Healthcare";
        public string FromEmail { get; set; } = string.Empty;
        public int VerificationOtpExpirationMinutes { get; set; } = 10;
        public int PasswordResetOtpExpirationMinutes { get; set; } = 15;
        public string ApplicationBaseUrl { get; set; } = string.Empty;
        public int OtpLength { get; set; } = 6;
        public int MaxOtpResendAttemptsPerHour { get; set; } = 5;
    }
}