using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums;
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

        public async Task<string> GenerateAndStoreOtpAsync(Guid userId, string email, VerificationTypes verificationType, string? ipAddress = null)
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

        public async Task<bool> ValidateOtpAsync(string email, string otpCode, VerificationTypes verificationType)
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
            if (verification.AttemptCount > 5)
            {
                verification.IsUsed = true; // Lock the OTP
                await _context.SaveChangesAsync();
                _logger.LogWarning("OTP locked due to too many attempts for email {Email}", email);
                return false;
            }

            // Validate OTP code
            if (verification.OtpCode != otpCode)
            {
                await _context.SaveChangesAsync();
                _logger.LogWarning("Invalid OTP attempt for email {Email}", email);
                return false;
            }

            // Mark as used
            verification.IsUsed = true;
            verification.VerifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("OTP validated successfully for email {Email}, type {Type}", email, verificationType);
            return true;
        }

        public async Task<bool> CanResendOtpAsync(string email)
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            var recentOtpCount = await _context.Set<EmailVerification>()
                .Where(v => v.Email == email && v.CreatedAt >= oneHourAgo)
                .CountAsync();

            var canResend = recentOtpCount < _emailSettings.MaxOtpResendAttemptsPerHour;

            if (!canResend)
            {
                _logger.LogWarning("Too many OTP resend attempts for email {Email}", email);
            }

            return canResend;
        }

        public async Task InvalidateAllOtpsAsync(Guid userId, VerificationTypes verificationType)
        {
            var existingOtps = await _context.Set<EmailVerification>()
                .Where(v => v.UserId == userId
                    && v.VerificationType == verificationType
                    && !v.IsUsed)
                .ToListAsync();

            foreach (var otp in existingOtps)
            {
                otp.IsUsed = true;
            }

            if (existingOtps.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Invalidated {Count} OTPs for user {UserId}, type {Type}",
                    existingOtps.Count, userId, verificationType);
            }
        }

        public string GenerateSecureOtp(int length)
        {
            const string digits = "0123456789";
            var otp = new char[length];

            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[length];
            rng.GetBytes(randomBytes);

            for (int i = 0; i < length; i++)
            {
                otp[i] = digits[randomBytes[i] % digits.Length];
            }

            return new string(otp);
        }
    }
}