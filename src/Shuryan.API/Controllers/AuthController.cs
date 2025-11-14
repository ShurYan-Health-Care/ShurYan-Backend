using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Auth;
using Shuryan.Application.Interfaces;
using System.Security.Claims;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        #region Registration
        [HttpPost("register/patient")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RegisterPatient([FromBody] RegisterPatientRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration request for email: {Email}", dto.Email);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "Validation failed",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Patient registration attempt for email: {Email}", dto.Email);

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.RegisterPatientAsync(dto, ipAddress);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Registration failed for {Email}: {Message}", dto.Email, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("Patient registered successfully: {Email}", dto.Email);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during patient registration for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during registration",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("register/doctor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RegisterDoctor([FromBody] RegisterDoctorRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration request for email: {Email}", dto.Email);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "Validation failed",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Doctor registration attempt for email: {Email}", dto.Email);

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.RegisterDoctorAsync(dto, ipAddress);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Registration failed for {Email}: {Message}", dto.Email, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("Doctor registered successfully: {Email}", dto.Email);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during doctor registration for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during registration",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("register/laboratory")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RegisterLaboratory([FromBody] RegisterLaboratoryRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration request for email: {Email}", dto.Email);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "Validation failed",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Laboratory registration attempt for email: {Email}", dto.Email);

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.RegisterLaboratoryAsync(dto, ipAddress);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Registration failed for {Email}: {Message}", dto.Email, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("Laboratory registered successfully: {Email}", dto.Email);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during laboratory registration for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during registration",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("register/pharmacy")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RegisterPharmacy([FromBody] RegisterPharmacyRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration request for email: {Email}", dto.Email);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "Validation failed",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Pharmacy registration attempt for email: {Email}", dto.Email);

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.RegisterPharmacyAsync(dto, ipAddress);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Registration failed for {Email}: {Message}", dto.Email, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("Pharmacy registered successfully: {Email}", dto.Email);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during pharmacy registration for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during registration",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("register/verifier")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RegisterVerifier([FromBody] RegisterVerifierRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration request for email: {Email}", dto.Email);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "Validation failed",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Verifier registration attempt for email: {Email}", dto.Email);

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.RegisterVerifierAsync(dto, ipAddress);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Registration failed for {Email}: {Message}", dto.Email, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("Verifier registered successfully: {Email}", dto.Email);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during verifier registration for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during registration",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Email Verification
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> VerifyEmail([FromBody] VerifyEmailRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid email verification request for email: {Email}", dto.Email);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Email verification attempt for: {Email}", dto.Email);

            try
            {
                var result = await _authService.VerifyEmailAsync(dto);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Email verification failed for {Email}: {Message}", dto.Email, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("Email verified successfully: {Email}", dto.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during email verification",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> ResendVerificationOtp([FromBody] ResendOtpRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid resend verification request for email: {Email}", dto.Email);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Resend verification OTP request for: {Email}", dto.Email);

            try
            {
                var result = await _authService.ResendVerificationOtpAsync(dto);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Resend verification OTP failed for {Email}: {Message}", dto.Email, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("Verification OTP resent successfully: {Email}", dto.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during resend verification OTP for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while resending verification OTP",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Login
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login request for email: {Email}", dto.Email);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.LoginAsync(dto, ipAddress);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Login failed for {Email}: {Message}", dto.Email, result.Message);
                    return StatusCode(result.StatusCode ?? 401, result);
                }

                _logger.LogInformation("User logged in successfully: {Email}", dto.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during login",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("google-login")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> GoogleLogin([FromBody] GoogleLoginRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid Google login request");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Google login attempt");

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.GoogleLoginAsync(dto, ipAddress);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Google login failed: {Message}", result.Message);
                    return StatusCode(result.StatusCode ?? 401, result);
                }

                _logger.LogInformation("Google login successful, Status: {StatusCode}", result.StatusCode);
                return StatusCode(result.StatusCode ?? 200, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during Google login",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Password Management
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid forgot password request for email: {Email}", dto.Email);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Forgot password request for email: {Email}", dto.Email);

            try
            {
                var result = await _authService.ForgotPasswordAsync(dto);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Forgot password failed for {Email}: {Message}", dto.Email, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("Password reset OTP sent successfully: {Email}", dto.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during password reset request",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] VerifyResetOtpRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid reset password request for email: {Email}", dto.Email);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Reset password attempt for email: {Email}", dto.Email);

            try
            {
                var result = await _authService.VerifyResetOtpAndResetPasswordAsync(dto);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Reset password failed for {Email}: {Message}", dto.Email, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("Password reset successfully: {Email}", dto.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reset password for {Email}", dto.Email);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during password reset",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto)
        {
            var userId = GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized change password attempt");
                return Unauthorized(ApiResponse<object>.Failure("User not authenticated", statusCode: 401));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid change password request for user: {UserId}", userId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Change password attempt for user: {UserId}", userId);

            try
            {
                var result = await _authService.ChangePasswordAsync(userId, dto);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Change password failed for user {UserId}: {Message}", userId, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during change password for user {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during password change",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Token Management
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid refresh token request");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Refresh token attempt");

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.RefreshTokenAsync(dto, ipAddress);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Refresh token failed: {Message}", result.Message);
                    return StatusCode(result.StatusCode ?? 401, result);
                }

                _logger.LogInformation("Token refreshed successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during token refresh",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest dto)
        {
            var userId = GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid logout request for user: {UserId}", userId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure("Validation failed", errors, 400));
            }

            _logger.LogInformation("Logout attempt for user: {UserId}", userId);

            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.LogoutAsync(dto.RefreshToken, ipAddress);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Logout failed for user {UserId}: {Message}", userId, result.Message);
                    return StatusCode(result.StatusCode ?? 400, result);
                }

                _logger.LogInformation("User logged out successfully: {UserId}", userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred during logout",
                    new[] { ex.Message },
                    500
                ));
            }
        }
        #endregion

        #region Helper Methods
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Guid.Empty;
            }

            return userId;
        }

        private string? GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        #endregion
    }
}