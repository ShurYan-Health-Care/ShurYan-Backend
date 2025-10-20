using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Requests.Auth;
using Shuryan.Application.Services.Auth;
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

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.LoginAsync(dto, ipAddress);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 401, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Register a new patient account
        /// </summary>
        [HttpPost("register/patient")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequest dto)
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.RegisterPatientAsync(dto, ipAddress);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, result);
            }

            return StatusCode(201, result);
        }

        /// <summary>
        /// Register a new doctor account
        /// </summary>
        [HttpPost("register/doctor")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorRequest dto)
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.RegisterDoctorAsync(dto, ipAddress);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, result);
            }

            return StatusCode(201, result);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest dto)
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.RefreshTokenAsync(dto, ipAddress);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 401, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Change password for authenticated user
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto)
        {
            var userId = GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _authService.ChangePasswordAsync(userId, dto);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Request password reset (sends reset email)
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Reset password using reset token
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Logout (revoke refresh token)
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest dto)
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.LogoutAsync(dto.RefreshToken, ipAddress);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Register a new laboratory account
        /// </summary>
        [HttpPost("register/laboratory")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterLaboratory([FromBody] RegisterLaboratoryRequest dto)
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.RegisterLaboratoryAsync(dto, ipAddress);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, result);
            }

            return StatusCode(201, result);
        }

        /// <summary>
        /// Register a new pharmacy account
        /// </summary>
        [HttpPost("register/pharmacy")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterPharmacy([FromBody] RegisterPharmacyRequest dto)
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.RegisterPharmacyAsync(dto, ipAddress);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, result);
            }

            return StatusCode(201, result);
        }

        /// <summary>
        /// Logout from all devices (revoke all refresh tokens)
        /// </summary>
        //[Authorize]
        //[HttpPost("logout-all")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //public async Task<IActionResult> LogoutFromAllDevices()
        //{
        //    var userId = GetCurrentUserId();

        //    if (userId == Guid.Empty)
        //    {
        //        return Unauthorized(new { message = "User not authenticated" });
        //    }

        //    var result = await _authService.LogoutFromAllDevicesAsync(userId);

        //    if (!result.IsSuccess)
        //    {
        //        return StatusCode(result.StatusCode ?? 400, result);
        //    }

        //    return Ok(result);
        //}

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _authService.GetCurrentUserAsync(userId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 404, result);
            }

            return Ok(result);
        }

        #region Helper Methods

        /// <summary>
        /// Gets the current user's ID from JWT claims
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Guid.Empty;
            }

            return userId;
        }

        /// <summary>
        /// Gets the client's IP address
        /// </summary>
        private string? GetIpAddress()
        {
            // Check for X-Forwarded-For header (if behind proxy/load balancer)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            // Otherwise, get the remote IP address
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        #endregion
    }
}