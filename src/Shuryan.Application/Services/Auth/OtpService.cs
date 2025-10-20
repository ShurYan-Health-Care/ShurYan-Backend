using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuryan.Core.Entities.System;
using Shuryan.Infrastructure.Data;
using Shuryan.Shared.Configurations;

namespace Shuryan.Application.Services.Auth
{
    public class OtpService : IOtpService
    {
        private readonly ShuryanDbContext _context;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            ShuryanDbContext context,
            IOptions<EmailSettings> emailSettings,
            ILogger<OtpService> logger)
        {
            _context = context;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<string> GenerateAndStoreOtpAsync(
            Guid userId,
            string email,
            string verificationType,
            string? ipAddress = null)
        {
            // Generate secure 6-digit OTP
            var otpCode = GenerateSecureOtp(_emailSettings.OtpLength);

            // Determine expiration based on verification type
            var expirationMinutes = verificationType == VerificationTypes.PasswordReset
                ? _emailSettings.PasswordResetOtpExpirationMinutes
                : _emailSettings.VerificationOtpExpirationMinutes;

            // Invalidate previous OTPs of the same type
            await InvalidateAllOtpsAsync(userId, verificationType);

            // Create new OTP record
            var verification = new EmailVerification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = email,
                OtpCode = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                VerificationType = verificationType,
                RequestedFromIp = ipAddress,
                IsUsed = false,
                AttemptCount = 0
            };

            _context.Set<EmailVerification>().Add(verification);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "OTP generated for user {UserId}, type {Type}, expires at {ExpiresAt}",
                userId, verificationType, verification.ExpiresAt);

            return otpCode;
        }

        public async Task<bool> ValidateOtpAsync(string email, string otpCode, string verificationType)
        {
            var verification = await _context.Set<EmailVerification>()
                .Where(v => v.Email == email
                    && v.VerificationType == verificationType
                    && !v.IsUsed
                    && v.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (verification == null)
            {
                _logger.LogWarning("No valid OTP found for email {Email}, type {Type}", email, verificationType);
                return false;
            }

            // Increment attempt count
            verification.AttemptCount++;

            // Check if too many attempts
            if (verification.At
}
