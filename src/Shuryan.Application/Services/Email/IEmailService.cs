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
        Task<bool> SendVerificationOtpAsync(string toEmail, string toName, string otpCode);
        Task<bool> SendPasswordResetOtpAsync(string toEmail, string toName, string otpCode);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string toName);
        Task<bool> SendPasswordChangedNotificationAsync(string toEmail, string toName);
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null);
    }
}