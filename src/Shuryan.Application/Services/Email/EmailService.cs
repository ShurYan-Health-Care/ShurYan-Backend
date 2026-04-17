using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuryan.Shared.Configurations;

namespace Shuryan.Application.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendVerificationOtpAsync(string toEmail, string toName, string otpCode)
        {
            var subject = "Verify Your Email - Shuryan Healthcare";
            var htmlBody = GetVerificationOtpEmailTemplate(toName, otpCode);

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendPasswordResetOtpAsync(string toEmail, string toName, string otpCode)
        {
            var subject = "Reset Your Password - Shuryan Healthcare";
            var htmlBody = GetPasswordResetOtpEmailTemplate(toName, otpCode);

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string toName)
        {
            var subject = "Welcome to Shuryan Healthcare!";
            var htmlBody = GetWelcomeEmailTemplate(toName);

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendPasswordChangedNotificationAsync(string toEmail, string toName)
        {
            var subject = "Password Changed Successfully - Shuryan Healthcare";
            var htmlBody = GetPasswordChangedEmailTemplate(toName);

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null)
        {
            try
            {
                using var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    Timeout = 30000 // 30 seconds
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(new MailAddress(toEmail));

                // Add plain text alternative if provided
                if (!string.IsNullOrEmpty(plainTextBody))
                {
                    var plainView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
                    mailMessage.AlternateViews.Add(plainView);
                }

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email to {Email}: {Message}", toEmail, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {Email}: {Message}", toEmail, ex.Message);
                return false;
            }
        }

        #region Email Templates

        private string GetVerificationOtpEmailTemplate(string userName, string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #334155; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0f766e; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background-color: #f1f5f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #0f766e; letter-spacing: 5px; text-align: center; padding: 20px; background-color: #f0fdfa; border-radius: 8px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #64748b; font-size: 12px; }}
        .warning {{ background-color: #ecfdf5; padding: 15px; border-left: 4px solid #10b981; margin-top: 20px; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🏥 Shuryan Healthcare</h1>
            <p>Email Verification</p>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Thank you for registering with Shuryan Healthcare! Please use the following OTP code to verify your email address:</p>
            
            <div class='otp-code'>{otpCode}</div>
            
            <p>This code will expire in <strong>{_emailSettings.VerificationOtpExpirationMinutes} minutes</strong>.</p>
            
            <div class='warning'>
                <strong>Security Notice:</strong> If you didn't request this verification, please ignore this email. Never share this code with anyone.
            </div>
            
            <p style='margin-top: 20px;'>Best regards,<br>The Shuryan Healthcare Team</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Shuryan Healthcare. All rights reserved.</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetOtpEmailTemplate(string userName, string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #334155; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0f172a; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background-color: #f1f5f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #0f172a; letter-spacing: 5px; text-align: center; padding: 20px; background-color: #e2e8f0; border-radius: 8px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #64748b; font-size: 12px; }}
        .warning {{ background-color: #f0fdfa; padding: 15px; border-left: 4px solid: #14b8a6; margin-top: 20px; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🏥 Shuryan Healthcare</h1>
            <p>Password Reset Request</p>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>We received a request to reset your password. Use the following OTP code to proceed:</p>
            
            <div class='otp-code'>{otpCode}</div>
            
            <p>This code will expire in <strong>{_emailSettings.PasswordResetOtpExpirationMinutes} minutes</strong>.</p>
            
            <div class='warning'>
                <strong>Security Alert:</strong> If you didn't request a password reset, please ignore this email and ensure your account is secure. Consider changing your password if you suspect unauthorized access.
            </div>
            
            <p style='margin-top: 20px;'>Best regards,<br>The Shuryan Healthcare Team</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Shuryan Healthcare. All rights reserved.</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetWelcomeEmailTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #334155; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #059669; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background-color: #f1f5f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .footer {{ text-align: center; padding: 20px; color: #64748b; font-size: 12px; }}
        .cta-button {{ display: inline-block; padding: 12px 30px; background-color: #059669; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome to Shuryan Healthcare!</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Your account has been successfully verified! We're thrilled to have you join the Shuryan Healthcare community.</p>
            
            <p>You can now:</p>
            <ul>
                <li>Book appointments with verified doctors</li>
                <li>Order lab tests from certified laboratories</li>
                <li>Get medications delivered from trusted pharmacies</li>
                <li>Access your complete medical history</li>
            </ul>
            
            <a href='{_emailSettings.ApplicationBaseUrl}' class='cta-button'>Get Started</a>
            
            <p style='margin-top: 30px;'>If you have any questions, feel free to reach out to our support team.</p>
            
            <p>Best regards,<br>The Shuryan Healthcare Team</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Shuryan Healthcare. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordChangedEmailTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #334155; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #059669; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background-color: #f1f5f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .footer {{ text-align: center; padding: 20px; color: #64748b; font-size: 12px; }}
        .warning {{ background-color: #fef7ff; padding: 15px; border-left: 4px solid #0891b2; margin-top: 20px; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Changed</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Your password has been successfully changed on <strong>{DateTime.UtcNow:MMMM dd, yyyy 'at' HH:mm} UTC</strong>.</p>
            
            <div class='warning'>
                <strong>Didn't make this change?</strong><br>
                If you didn't change your password, please contact our support team immediately to secure your account.
            </div>
            
            <p style='margin-top: 20px;'>Best regards,<br>The Shuryan Healthcare Team</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Shuryan Healthcare. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
        #endregion
    }
}