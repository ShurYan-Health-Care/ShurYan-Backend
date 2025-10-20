using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Auth;
using Shuryan.Application.DTOs.Responses.Auth;
using Shuryan.Application.Services.Token;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Interfaces.UnitOfWork;
using Shuryan.Shared.Configurations;

namespace Shuryan.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterPatientAsync(RegisterPatientRequest dto, string? ipAddress = null)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Email already registered", new[] { "A user with this email already exists" }, 400);
                }

                // Create Patient entity
                var patient = new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    EmailConfirmed = false, // Set to true if not using email confirmation
                    CreatedAt = DateTime.UtcNow
                };

                // Create user with password
                var result = await _userManager.CreateAsync(patient, dto.Password);

                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Registration failed", result.Errors.Select(e => e.Description), 400);
                }

                // Ensure Patient role exists
                await EnsureRoleExistsAsync(UserRole.Patient);

                // Assign Patient role
                await _userManager.AddToRoleAsync(patient, UserRole.Patient.ToString());

                // Generate tokens
                var authResponse = await GenerateAuthResponseAsync(patient, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(authResponse, "Patient registered successfully", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponseDto>.Failure("An error occurred during registration", new[] { ex.Message }, 500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterDoctorAsync(RegisterDoctorRequest dto, string? ipAddress = null)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Email already registered", new[] { "A user with this email already exists" }, 400);
                }

                // Create Doctor entity
                var doctor = new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    MedicalSpecialty = dto.MedicalSpecialty,
                    VerificationStatus = VerificationStatus.Unverified, // Requires verification
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                // Create user with password
                var result = await _userManager.CreateAsync(doctor, dto.Password);

                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Registration failed", result.Errors.Select(e => e.Description), 400);
                }

                // Ensure Doctor role exists
                await EnsureRoleExistsAsync(UserRole.Doctor);

                // Assign Doctor role
                await _userManager.AddToRoleAsync(doctor, UserRole.Doctor.ToString());

                // Generate tokens
                var authResponse = await GenerateAuthResponseAsync(doctor, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(authResponse, "Doctor registered successfully. Please submit verification documents.", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponseDto>.Failure("An error occurred during registration", new[] { ex.Message }, 500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterLaboratoryAsync(RegisterLaboratoryRequest dto, string? ipAddress = null)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Email already registered", new[] { "A user with this email already exists" }, 400);
                }

                // Create Address
                var address = new Address
                {
                    Id = Guid.NewGuid(),
                    Street = dto.Address.Street,
                    City = dto.Address.City,
                    Governorate = dto.Address.Governorate,
                    BuildingNumber = dto.Address.BuildingNumber,
                    Latitude = dto.Address.Latitude,
                    Longitude = dto.Address.Longitude,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Addresses.AddAsync(address);

                // Create Laboratory entity
                var laboratory = new Laboratory
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Lab", // Laboratory doesn't have FirstName/LastName, using placeholder
                    LastName = dto.Name,
                    Name = dto.Name,
                    Email = dto.Email,
                    UserName = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Description = dto.Description,
                    WhatsAppNumber = dto.WhatsAppNumber,
                    Website = dto.Website,
                    OffersHomeSampleCollection = dto.OffersHomeSampleCollection,
                    HomeSampleCollectionFee = dto.HomeSampleCollectionFee,
                    AddressId = address.Id,
                    LaboratoryStatus = Status.Active,
                    VerificationStatus = VerificationStatus.Unverified,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                // Create user with password
                var result = await _userManager.CreateAsync(laboratory, dto.Password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Registration failed",
                        result.Errors.Select(e => e.Description),
                        400);
                }

                // Ensure Laboratory role exists
                await EnsureRoleExistsAsync(UserRole.Laboratory);

                // Assign Laboratory role
                await _userManager.AddToRoleAsync(laboratory, UserRole.Laboratory.ToString());

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                // Generate tokens
                var authResponse = await GenerateAuthResponseAsync(laboratory, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(authResponse, "Laboratory registered successfully. Please submit verification documents.", 201);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse<AuthResponseDto>.Failure("An error occurred during registration", new[] { ex.Message }, 500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterPharmacyAsync(RegisterPharmacyRequest dto, string? ipAddress = null)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Email already registered", new[] { "A user with this email already exists" }, 400);
                }

                // Create Address
                var address = new Address
                {
                    Id = Guid.NewGuid(),
                    Street = dto.Address.Street,
                    City = dto.Address.City,
                    Governorate = dto.Address.Governorate,
                    BuildingNumber = dto.Address.BuildingNumber,
                    Latitude = dto.Address.Latitude,
                    Longitude = dto.Address.Longitude,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Addresses.AddAsync(address);

                // Create Pharmacy entity
                var pharmacy = new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Pharmacy", // Pharmacy doesn't have FirstName/LastName, using placeholder
                    LastName = dto.Name,
                    Name = dto.Name,
                    Email = dto.Email,
                    UserName = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Description = dto.Description,
                    WhatsAppNumber = dto.WhatsAppNumber,
                    Website = dto.Website,
                    OffersDelivery = dto.OffersDelivery,
                    AddressId = address.Id,
                    PharmacyStatus = Status.Active,
                    VerificationStatus = VerificationStatus.Unverified,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                // Create user with password
                var result = await _userManager.CreateAsync(pharmacy, dto.Password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<AuthResponseDto>.Failure("Registration failed", result.Errors.Select(e => e.Description), 400);
                }

                // Ensure Pharmacy role exists
                await EnsureRoleExistsAsync(UserRole.Pharmacy);

                // Assign Pharmacy role
                await _userManager.AddToRoleAsync(pharmacy, UserRole.Pharmacy.ToString());

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                // Generate tokens
                var authResponse = await GenerateAuthResponseAsync(pharmacy, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(
                    authResponse,
                    "Pharmacy registered successfully. Please submit verification documents.",
                    201);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred during registration",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequest dto, string? ipAddress = null)
        {
            try
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(dto.Email);

                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Invalid credentials", new[] { "Email or password is incorrect" }, 401);
                }

                // Check if user is deleted (soft delete)
                if (user.IsDeleted)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Account deactivated", new[] { "This account has been deactivated" }, 403);
                }

                // Check if account is locked
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return ApiResponse<AuthResponseDto>.Failure("Account locked", new[] { "Account is temporarily locked due to multiple failed login attempts" }, 403);
                }

                // Attempt sign in
                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        return ApiResponse<AuthResponseDto>.Failure("Account locked", new[] { "Account is temporarily locked due to multiple failed login attempts" }, 403);
                    }
                    return ApiResponse<AuthResponseDto>.Failure("Invalid credentials", new[] { "Email or password is incorrect" }, 401);
                }

                // Generate tokens
                var authResponse = await GenerateAuthResponseAsync(user, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(authResponse, "Login successful", 200);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponseDto>.Failure("An error occurred during login", new[] { ex.Message }, 500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(
    RefreshTokenRequest dto,
    string? ipAddress = null)
        {
            try
            {
                // Validate access token (even if expired)
                var userId = _tokenService.GetUserIdFromToken(dto.AccessToken);

                if (userId == null)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Invalid token",
                        new[] { "Invalid access token" },
                        401);
                }

                // Find refresh token in database
                var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(dto.RefreshToken);

                if (refreshToken == null || refreshToken.UserId != userId)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Invalid refresh token",
                        new[] { "Refresh token not found or does not match user" },
                        401);
                }

                // Check if refresh token is active
                if (!refreshToken.IsActive)
                {
                    if (refreshToken.IsRevoked)
                    {
                        // Possible security breach - revoke all user tokens
                        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                            userId.Value,
                            "Attempted reuse of revoked token");

                        return ApiResponse<AuthResponseDto>.Failure(
                            "Invalid refresh token",
                            new[] { "This refresh token has been revoked" },
                            401);
                    }

                    return ApiResponse<AuthResponseDto>.Failure(
                        "Expired refresh token",
                        new[] { "Refresh token has expired" },
                        401);
                }

                // Get user
                var user = await _userManager.FindByIdAsync(userId.Value.ToString());

                if (user == null || user.IsDeleted)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "User not found",
                        new[] { "User account not found or deactivated" },
                        404);
                }

                // Revoke old refresh token
                await _unitOfWork.RefreshTokens.RevokeTokenAsync(
                    dto.RefreshToken,
                    "Replaced by new token",
                    ipAddress);

                // Generate new tokens
                var authResponse = await GenerateAuthResponseAsync(user, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(
                    authResponse,
                    "Token refreshed successfully",
                    200);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred while refreshing token",
                    new[] { ex.Message },
                    500);
            }
        }

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(
    User user,
    string? ipAddress = null,
    bool rememberMe = false)
        {
            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate access token
            var accessToken = _tokenService.GenerateAccessToken(
                user.Id,
                user.Email!,
                roles);

            // Generate refresh token
            var refreshTokenString = _tokenService.GenerateRefreshToken();

            // Calculate expiration times
            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(
                rememberMe ? _jwtSettings.RefreshTokenExpirationDays * 2 : _jwtSettings.RefreshTokenExpirationDays);

            // Store refresh token in database
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshTokenString,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshTokenExpiration,
                CreatedByIp = ipAddress
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            // Build user info
            var userInfo = await BuildUserInfoAsync(user, roles);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                AccessTokenExpiresAt = accessTokenExpiration,
                RefreshTokenExpiresAt = refreshTokenExpiration,
                TokenType = "Bearer",
                User = userInfo
            };
        }

        private async Task<UserInfoDto> BuildUserInfoAsync(User user, IList<string> roles)
        {
            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles
            };

            // Add user type-specific information
            if (user is Doctor doctor)
            {
                userInfo.AdditionalInfo = new Dictionary<string, object>
                {
                    { "MedicalSpecialty", doctor.MedicalSpecialty.ToString() },
                    { "VerificationStatus", doctor.VerificationStatus.ToString() }
                };
            }
            else if (user is Laboratory lab)
            {
                userInfo.AdditionalInfo = new Dictionary<string, object>
                {
                    { "Name", lab.Name },
                    { "VerificationStatus", lab.VerificationStatus.ToString() },
                    { "OffersHomeSampleCollection", lab.OffersHomeSampleCollection }
                };
            }
            else if (user is Pharmacy pharmacy)
            {
                userInfo.AdditionalInfo = new Dictionary<string, object>
                {
                    { "Name", pharmacy.Name },
                    { "VerificationStatus", pharmacy.VerificationStatus.ToString() },
                    { "OffersDelivery", pharmacy.OffersDelivery }
                };
            }
            else if (user is Patient patient)
            {
                userInfo.AdditionalInfo = new Dictionary<string, object>
                {
                    { "BirthDate", patient.BirthDate?.ToString("yyyy-MM-dd") ?? "N/A" },
                    { "Gender", patient.Gender?.ToString() ?? "N/A" }
                };
            }

            return userInfo;
        }

        private async Task EnsureRoleExistsAsync(UserRole role)
        {
            var roleName = role.ToString();
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                var newRole = new Role
                {
                    Name = roleName,
                    UserRole = role
                };
                await _roleManager.CreateAsync(newRole);
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.IsDeleted)
                {
                    return ApiResponse<bool>.Failure(
                        "User not found",
                        new[] { "User account not found" },
                        404);
                }

                var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.Failure(
                        "Password change failed",
                        result.Errors.Select(e => e.Description),
                        400);
                }

                // Optionally revoke all refresh tokens for security
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId, "Password changed");

                return ApiResponse<bool>.Success(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure(
                    "An error occurred while changing password",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);

                // Don't reveal whether user exists or not (security)
                if (user == null)
                {
                    return ApiResponse<bool>.Success(
                        true,
                        "If your email exists in our system, you will receive a password reset link");
                }

                // Generate password reset token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // TODO: Send email with reset link
                // await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

                return ApiResponse<bool>.Success(
                    true,
                    "If your email exists in our system, you will receive a password reset link");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure(
                    "An error occurred while processing your request",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);

                if (user == null)
                {
                    return ApiResponse<bool>.Failure(
                        "Invalid request",
                        new[] { "Invalid email or token" },
                        400);
                }

                var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.Failure(
                        "Password reset failed",
                        result.Errors.Select(e => e.Description),
                        400);
                }

                // Revoke all refresh tokens
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, "Password reset");

                return ApiResponse<bool>.Success(true, "Password reset successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure(
                    "An error occurred while resetting password",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken, string? ipAddress = null)
        {
            try
            {
                await _unitOfWork.RefreshTokens.RevokeTokenAsync(
                    refreshToken,
                    "User logout",
                    ipAddress);

                return ApiResponse<bool>.Success(true, "Logged out successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure(
                    "An error occurred during logout",
                    new[] { ex.Message },
                    500);
            }
        }

        // Pseudocode / Plan:
        // 1. Try to find the user by the provided userId using _userManager.FindByIdAsync.
        // 2. If user is null -> return ApiResponse<UserInfoDto>.Failure with 404 ("User not found").
        // 3. If user.IsDeleted -> return ApiResponse<UserInfoDto>.Failure with 404 (consistent with other methods).
        // 4. Retrieve roles for the user via _userManager.GetRolesAsync.
        // 5. Build UserInfoDto by calling the existing BuildUserInfoAsync(user, roles).
        // 6. Return ApiResponse<UserInfoDto>.Success with the built user info and status 200.
        // 7. Wrap in try/catch and return a 500 failure with exception message on unexpected errors.
        public async Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    return ApiResponse<UserInfoDto>.Failure("User not found", new[] { "User account not found" }, 404);
                }

                if (user.IsDeleted)
                {
                    return ApiResponse<UserInfoDto>.Failure("User not found", new[] { "User account not found or deactivated" }, 404);
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userInfo = await BuildUserInfoAsync(user, roles);

                return ApiResponse<UserInfoDto>.Success(userInfo, "User retrieved successfully", 200);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserInfoDto>.Failure("An error occurred while retrieving user", new[] { ex.Message }, 500);
            }
        }
    }
}
