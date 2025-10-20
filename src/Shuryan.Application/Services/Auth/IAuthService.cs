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
    /// <summary>
    /// Service interface for authentication operations
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new patient
        /// </summary>
        Task<ApiResponse<AuthResponseDto>> RegisterPatientAsync(RegisterPatientRequest dto, string? ipAddress = null);

        /// <summary>
        /// Registers a new doctor
        /// </summary>
        Task<ApiResponse<AuthResponseDto>> RegisterDoctorAsync(RegisterDoctorRequest dto, string? ipAddress = null);

        /// <summary>
        /// Registers a new laboratory
        /// </summary>
        Task<ApiResponse<AuthResponseDto>> RegisterLaboratoryAsync(RegisterLaboratoryRequest dto, string? ipAddress = null);

        /// <summary>
        /// Registers a new pharmacy
        /// </summary>
        Task<ApiResponse<AuthResponseDto>> RegisterPharmacyAsync(RegisterPharmacyRequest dto, string? ipAddress = null);

        /// <summary>
        /// Authenticates a user and returns tokens
        /// </summary>
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequest dto, string? ipAddress = null);

        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequest dto, string? ipAddress = null);

        /// <summary>
        /// Changes user password
        /// </summary>
        Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest dto);

        /// <summary>
        /// Initiates password reset process
        /// </summary>
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest dto);

        /// <summary>
        /// Completes password reset
        /// </summary>
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest dto);

        /// <summary>
        /// Logs out user by revoking refresh token
        /// </summary>
        Task<ApiResponse<bool>> LogoutAsync(string refreshToken, string? ipAddress = null);

        /// <summary>
        /// Gets current user information
        /// </summary>
        Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(Guid userId);
    }
}