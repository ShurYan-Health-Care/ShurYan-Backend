using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Auth;
using Shuryan.Application.DTOs.Responses.Auth;

namespace Shuryan.Application.Services.Auth
{
    public interface IAuthService
    {
        // Registration
        Task<ApiResponse<AuthResponseDto>> RegisterPatientAsync(RegisterPatientRequest dto, string? ipAddress = null);
        Task<ApiResponse<AuthResponseDto>> RegisterDoctorAsync(RegisterDoctorRequest dto, string? ipAddress = null);
        Task<ApiResponse<AuthResponseDto>> RegisterLaboratoryAsync(RegisterLaboratoryRequest dto, string? ipAddress = null);
        Task<ApiResponse<AuthResponseDto>> RegisterPharmacyAsync(RegisterPharmacyRequest dto, string? ipAddress = null);

        // Email Verification
        Task<ApiResponse<bool>> VerifyEmailAsync(VerifyEmailRequest dto);
        Task<ApiResponse<bool>> ResendVerificationOtpAsync(ResendOtpRequest dto);

        // Login
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequest dto, string? ipAddress = null);

        // Google OAuth
        Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(GoogleLoginRequest dto, string? ipAddress = null);

        // Password Management
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest dto);
        Task<ApiResponse<bool>> VerifyResetOtpAndResetPasswordAsync(VerifyResetOtpRequest dto);
        Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest dto);

        // Token Management
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequest dto, string? ipAddress = null);
        Task<ApiResponse<bool>> LogoutAsync(string refreshToken, string? ipAddress = null);

        // User Info
        Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(Guid userId);
    }
}