using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums;
using Shuryan.Infrastructure.Data;
using Shuryan.Shared.Configurations;

namespace Shuryan.Application.Interfaces
{
    public interface IOtpService
    {
        Task<string> GenerateAndStoreOtpAsync(Guid userId, string email, VerificationTypes verificationType, string? ipAddress = null);
        Task<bool> ValidateOtpAsync(string email, string otpCode, VerificationTypes verificationType);
        Task<bool> CanResendOtpAsync(string email);
        Task InvalidateAllOtpsAsync(Guid userId, VerificationTypes verificationType);
        string GenerateSecureOtp(int length);
    }
}