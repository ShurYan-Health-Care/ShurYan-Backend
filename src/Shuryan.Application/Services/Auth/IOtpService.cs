using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.Services.Auth
{
    public interface IOtpService
    {
        Task<string> GenerateAndStoreOtpAsync(Guid userId, string email, string verificationType, string? ipAddress = null);
        Task<bool> ValidateOtpAsync(string email, string otpCode, string verificationType);
        Task<bool> CanResendOtpAsync(string email);
        Task InvalidateAllOtpsAsync(Guid userId, string verificationType);
    }
}
