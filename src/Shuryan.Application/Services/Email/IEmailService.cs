using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;


namespace Shuryan.Application.Services.Email
{
    public interface IEmailService
    {
        /// <summary>
        /// Send email verification OTP
        /// </summary>
        Task<bool> SendVerificationOtpAsync(string toEmail, string toName, string otpCode);

        /// <summary>
        /// Send password reset OTP
        /// </summary>
        Task<bool> SendPasswordResetOtpAsync(string toEmail, string toName, string otpCode);

        /// <summary>
        /// Send welcome email after successful registration
        /// </summary>
        Task<bool> SendWelcomeEmailAsync(string toEmail, string toName);

        /// <summary>
        /// Send password changed notification
        /// </summary>
        Task<bool> SendPasswordChangedNotificationAsync(string toEmail, string toName);

        /// <summary>
        /// Send generic email
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null);
    }
}