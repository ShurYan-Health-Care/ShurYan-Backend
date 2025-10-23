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
using Shuryan.Infrastructure.Data;
using Shuryan.Shared.Configurations;

namespace Shuryan.Application.Interfaces
{
    public interface IOtpService
    {
        Task<string> GenerateAndStoreOtpAsync(Guid userId, string email, string verificationType, string? ipAddress = null);
        Task<bool> ValidateOtpAsync(string email, string otpCode, string verificationType);
        Task<bool> CanResendOtpAsync(string email);
        Task InvalidateAllOtpsAsync(Guid userId, string verificationType);
        string GenerateSecureOtp(int length);
    }
}