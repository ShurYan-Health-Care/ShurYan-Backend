using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Auth;
using Shuryan.Application.DTOs.Responses.Auth;
using Shuryan.Application.Interfaces;
using Shuryan.Application.Services.Email;
using Shuryan.Application.Services.Token;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums;
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
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IGoogleOAuthService _googleOAuthService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            ITokenService tokenService,
            IOtpService otpService,
            IEmailService emailService,
            IGoogleOAuthService googleOAuthService,
            IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _otpService = otpService;
            _emailService = emailService;
            _googleOAuthService = googleOAuthService;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        #region Registration

        public async Task<ApiResponse<AuthResponseDto>> RegisterPatientAsync(RegisterPatientRequest dto, string? ipAddress = null)
        {
            try
            {
                // Check if email exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Email already registered", new[] { "A user with this email already exists" }, 400);
                }

                // Create Patient
                var patient = new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                // Create user with password
                var result = await _userManager.CreateAsync(patient, dto.Password);

                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Registration failed", result.Errors.Select(e => e.Description), 400);
                }

                // Assign Patient role
                await EnsureRoleExistsAsync(UserRole.Patient);
                await _userManager.AddToRoleAsync(patient, UserRole.Patient.ToString());

                // Generate and send OTP
                var otpCode = await _otpService.GenerateAndStoreOtpAsync(
                    patient.Id,
                    patient.Email,
                    VerificationTypes.EmailVerification,
                    ipAddress);

                await _emailService.SendVerificationOtpAsync(
                    patient.Email,
                    patient.FirstName,
                    otpCode);

                _logger.LogInformation("Patient registered successfully: {Email}", patient.Email);

                // Generate tokens (user can use app but with limited access until verified)
                var authResponse = await GenerateAuthResponseAsync(patient, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(
                    authResponse,
                    "Registration successful! Please check your email for the verification code.",
                    201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during patient registration");
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred during registration",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterDoctorAsync(RegisterDoctorRequest dto, string? ipAddress = null)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Email already registered",
                        new[] { "A user with this email already exists" },
                        400);
                }

                var doctor = new Doctor
                {
                    Id = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    MedicalSpecialty = dto.MedicalSpecialty,
                    VerificationStatus = VerificationStatus.Unverified,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(doctor, dto.Password);

                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Registration failed",
                        result.Errors.Select(e => e.Description),
                        400);
                }

                await EnsureRoleExistsAsync(UserRole.Doctor);
                await _userManager.AddToRoleAsync(doctor, UserRole.Doctor.ToString());

                // Send verification OTP
                var otpCode = await _otpService.GenerateAndStoreOtpAsync(
                    doctor.Id,
                    doctor.Email,
                    VerificationTypes.EmailVerification,
                    ipAddress);

                await _emailService.SendVerificationOtpAsync(
                    doctor.Email,
                    doctor.FirstName,
                    otpCode);

                _logger.LogInformation("Doctor registered successfully: {Email}", doctor.Email);

                var authResponse = await GenerateAuthResponseAsync(doctor, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(
                    authResponse,
                    "Registration successful! Please verify your email and submit verification documents.",
                    201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during doctor registration");
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred during registration",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterLaboratoryAsync(RegisterLaboratoryRequest dto, string? ipAddress = null)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Email already registered",
                        new[] { "A user with this email already exists" },
                        400);
                }


                var laboratory = new Laboratory
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Email = dto.Email,
                    UserName = dto.Email,
                    VerificationStatus = VerificationStatus.Unverified,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(laboratory, dto.Password);

                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Registration failed", result.Errors.Select(e => e.Description), 400);
                }

                await EnsureRoleExistsAsync(UserRole.Laboratory);
                await _userManager.AddToRoleAsync(laboratory, UserRole.Laboratory.ToString());

                // Send verification OTP
                var otpCode = await _otpService.GenerateAndStoreOtpAsync(
                    laboratory.Id,
                    laboratory.Email,
                    VerificationTypes.EmailVerification,
                    ipAddress);

                await _emailService.SendVerificationOtpAsync(
                    laboratory.Email,
                    laboratory.Name,
                    otpCode);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Laboratory registered successfully: {Email}", laboratory.Email);

                var authResponse = await GenerateAuthResponseAsync(laboratory, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(authResponse, "Registration successful! Please verify your email and submit verification documents.", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during laboratory registration");
                //return ApiResponse<AuthResponseDto>.Failure("An error occurred during registration", new[] { ex.Message }, 500);
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred during registration",
                    new[] { ex.InnerException?.Message ?? ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterPharmacyAsync(RegisterPharmacyRequest dto, string? ipAddress = null)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Email already registered",
                        new[] { "A user with this email already exists" },
                        400);
                }

                var pharmacy = new Pharmacy
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Email = dto.Email,
                    UserName = dto.Email,
                    VerificationStatus = VerificationStatus.Unverified,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(pharmacy, dto.Password);

                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Registration failed",
                        result.Errors.Select(e => e.Description),
                        400);
                }

                await EnsureRoleExistsAsync(UserRole.Pharmacy);
                await _userManager.AddToRoleAsync(pharmacy, UserRole.Pharmacy.ToString());

                // Send verification OTP
                var otpCode = await _otpService.GenerateAndStoreOtpAsync(
                    pharmacy.Id,
                    pharmacy.Email,
                    VerificationTypes.EmailVerification,
                    ipAddress);

                await _emailService.SendVerificationOtpAsync(
                    pharmacy.Email,
                    pharmacy.Name,
                    otpCode);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Pharmacy registered successfully: {Email}", pharmacy.Email);

                var authResponse = await GenerateAuthResponseAsync(pharmacy, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(
                    authResponse,
                    "Registration successful! Please verify your email and submit verification documents.",
                    201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during pharmacy registration");
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred during registration",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterVerifierAsync(RegisterVerifierRequest dto, string? ipAddress = null)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Email already registered",
                        new[] { "A user with this email already exists" },
                        400);
                }

                // TODO: Add admin validation when Admin module is ready
                // var admin = await _userManager.FindByIdAsync(dto.CreatedByAdminId.ToString());
                // if (admin == null || !await _userManager.IsInRoleAsync(admin, UserRole.Admin.ToString()))
                // {
                //     return ApiResponse<AuthResponseDto>.Failure("Unauthorized", new[] { "Only admins can create verifier accounts" }, 403);
                // }

                var verifier = new Verifier
                {
                    Id = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                    // TODO: Add CreatedByAdminId when Admin module is ready
                    // CreatedByAdminId = dto.CreatedByAdminId
                };

                var result = await _userManager.CreateAsync(verifier, dto.Password);

                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Registration failed",
                        result.Errors.Select(e => e.Description),
                        400);
                }

                await EnsureRoleExistsAsync(UserRole.Verifier);
                await _userManager.AddToRoleAsync(verifier, UserRole.Verifier.ToString());

                // Send verification OTP
                var otpCode = await _otpService.GenerateAndStoreOtpAsync(
                    verifier.Id,
                    verifier.Email,
                    VerificationTypes.EmailVerification,
                    ipAddress);

                await _emailService.SendVerificationOtpAsync(
                    verifier.Email,
                    verifier.FirstName,
                    otpCode);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Verifier registered successfully: {Email}", verifier.Email);

                var authResponse = await GenerateAuthResponseAsync(verifier, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(
                    authResponse,
                    "Verifier registration successful! Please verify your email.",
                    201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during verifier registration");
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred during registration",
                    new[] { ex.Message },
                    500);
            }
        }

        #endregion

        #region Email Verification

        public async Task<ApiResponse<AuthResponseDto>> VerifyEmailAsync(VerifyEmailRequest dto, string? ipAddress = null)
        {
            try
            {
                // Validate OTP
                var isValid = await _otpService.ValidateOtpAsync(
                    dto.Email,
                    dto.OtpCode,
                    VerificationTypes.EmailVerification);

                if (!isValid)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Invalid or expired OTP code",
                        new[] { "Please check your code or request a new one" },
                        400);
                }

                // Find user and mark email as confirmed
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("User not found", null, 404);
                }

                user.EmailConfirmed = true;
                user.EmailVerifiedAt = DateTime.UtcNow;
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = ipAddress;
                await _userManager.UpdateAsync(user);

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);

                _logger.LogInformation("Email verified successfully for user: {Email}", dto.Email);

                // Generate tokens
                var authResponse = await GenerateAuthResponseAsync(user, ipAddress);

                return ApiResponse<AuthResponseDto>.Success(
                    authResponse,
                    "Email verified successfully! Welcome to Shuryan Healthcare.",
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred during verification",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<bool>> ResendVerificationOtpAsync(ResendOtpRequest dto)
        {
            try
            {
                // Check rate limiting
                var canResend = await _otpService.CanResendOtpAsync(dto.Email);
                if (!canResend)
                {
                    return ApiResponse<bool>.Failure(
                        "Too many requests",
                        new[] { "Please wait before requesting another code" },
                        429);
                }

                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    // Don't reveal if user exists
                    return ApiResponse<bool>.Success(
                        true,
                        "If your email exists, you'll receive a verification code");
                }

                if (user.EmailConfirmed)
                {
                    return ApiResponse<bool>.Failure(
                        "Email already verified",
                        null,
                        400);
                }

                // Generate new OTP
                var otpCode = await _otpService.GenerateAndStoreOtpAsync(
                    user.Id,
                    user.Email,
                    VerificationTypes.EmailVerification);

                await _emailService.SendVerificationOtpAsync(
                    user.Email,
                    user.FirstName,
                    otpCode);

                _logger.LogInformation("Verification OTP resent to: {Email}", dto.Email);

                return ApiResponse<bool>.Success(
                    true,
                    "Verification code sent! Please check your email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification OTP");
                return ApiResponse<bool>.Failure(
                    "An error occurred",
                    new[] { ex.Message },
                    500);
            }
        }

        #endregion

        #region Login

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequest dto, string? ipAddress = null)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);

                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Invalid credentials", new[] { "Email or password is incorrect" }, 401);
                }

                // Check soft delete
                if (user.IsDeleted)
                {
                    return ApiResponse<AuthResponseDto>.Failure("Account deactivated", new[] { "This account has been deactivated" }, 403);
                }

                // Check email verification (except for OAuth accounts)
                if (!user.EmailConfirmed && !user.IsOAuthAccount)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Email not verified",
                        new[] { "Please verify your email before logging in. Check your inbox for the verification code." },
                        403);
                }

                // Check lockout
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Account locked",
                        new[] { "Too many failed attempts. Please try again later." },
                        403);
                }

                // Verify password
                var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    dto.Password,
                    lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        return ApiResponse<AuthResponseDto>.Failure("Account locked", new[] { "Too many failed attempts. Account locked for 15 minutes." }, 403);
                    }

                    return ApiResponse<AuthResponseDto>.Failure("Invalid credentials", new[] { "Email or password is incorrect" }, 401);
                }

                // Successful login - update tracking
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = ipAddress;
                await _userManager.UpdateAsync(user);

                // Generate tokens
                var authResponse = await GenerateAuthResponseAsync(user, ipAddress);

                _logger.LogInformation("User logged in successfully: {Email}", user.Email);

                return ApiResponse<AuthResponseDto>.Success(authResponse, "Login successful", 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred during login",
                    new[] { ex.Message },
                    500);
            }
        }

        #region Google OAuth

        public async Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(
            GoogleLoginRequest dto,
            string? ipAddress = null)
        {
            try
            {
                // Validate Google token
                var googleUser = await _googleOAuthService.ValidateGoogleTokenAsync(dto.IdToken);
                if (googleUser == null)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Invalid Google token",
                        new[] { "Unable to verify Google credentials" },
                        401);
                }

                // Check if user exists
                var user = await _userManager.FindByEmailAsync(googleUser.Email);

                if (user != null)
                {
                    // ==========================================
                    // Scenario 1: User Exists (Returning User)
                    // ==========================================
                    
                    if (user.IsDeleted)
                    {
                        return ApiResponse<AuthResponseDto>.Failure(
                            "Account deactivated",
                            new[] { "This account has been deactivated" },
                            403);
                    }

                    // Update OAuth info if not set
                    if (!user.IsOAuthAccount)
                    {
                        user.IsOAuthAccount = true;
                        user.OAuthProvider = "Google";
                        user.OAuthProviderId = googleUser.Sub;
                        user.ProfilePictureUrl = googleUser.Picture;
                    }

                    // Update login tracking
                    user.LastLoginAt = DateTime.UtcNow;
                    user.LastLoginIp = ipAddress;
                    user.EmailConfirmed = true; // Google emails are verified
                    user.EmailVerifiedAt ??= DateTime.UtcNow;

                    await _userManager.UpdateAsync(user);

                    var authResponse = await GenerateAuthResponseAsync(user, ipAddress);

                    _logger.LogInformation("User logged in via Google: {Email}", user.Email);

                    return ApiResponse<AuthResponseDto>.Success(
                        authResponse,
                        "Login successful",
                        200);
                }
                else
                {
                    // ==========================================
                    // New User - Check if UserType is provided
                    // ==========================================
                    
                    if (string.IsNullOrEmpty(dto.UserType))
                    {
                        // ==========================================
                        // Scenario 2: User Not Found, No Type Provided
                        // Return 404 to prompt user type selection
                        // ==========================================
                        
                        _logger.LogInformation("New Google user, awaiting user type selection: {Email}", googleUser.Email);
                        
                        return ApiResponse<AuthResponseDto>.Failure(
                            "User not found. Please select user type to register.",
                            new[] { "New user detected. User type selection required." },
                            404);
                    }

                    // ==========================================
                    // Scenario 3: New User, Type Provided
                    // Register the user
                    // ==========================================
                    
                    // Parse user type
                    UserRole userRole;
                    try
                    {
                        userRole = Enum.Parse<UserRole>(dto.UserType, true);
                    }
                    catch
                    {
                        return ApiResponse<AuthResponseDto>.Failure(
                            "Invalid user type",
                            new[] { "User type must be one of: Patient, Doctor, Pharmacy, Laboratory" },
                            400);
                    }

                    // Create appropriate user entity based on role
                    User newUser = userRole switch
                    {
                        UserRole.Doctor => new Doctor
                        {
                            Id = Guid.NewGuid(),
                            FirstName = googleUser.GivenName,
                            LastName = googleUser.FamilyName,
                            Email = googleUser.Email,
                            UserName = googleUser.Email,
                            EmailConfirmed = true,
                            EmailVerifiedAt = DateTime.UtcNow,
                            IsOAuthAccount = true,
                            OAuthProvider = "Google",
                            OAuthProviderId = googleUser.Sub,
                            ProfilePictureUrl = googleUser.Picture,
                            VerificationStatus = VerificationStatus.Unverified,
                            CreatedAt = DateTime.UtcNow
                        },
                        UserRole.Pharmacy => new Pharmacy
                        {
                            Id = Guid.NewGuid(),
                            Name = googleUser.Name,
                            Email = googleUser.Email,
                            UserName = googleUser.Email,
                            EmailConfirmed = true,
                            EmailVerifiedAt = DateTime.UtcNow,
                            IsOAuthAccount = true,
                            OAuthProvider = "Google",
                            OAuthProviderId = googleUser.Sub,
                            ProfilePictureUrl = googleUser.Picture,
                            VerificationStatus = VerificationStatus.Unverified,
                            CreatedAt = DateTime.UtcNow
                        },
                        UserRole.Laboratory => new Laboratory
                        {
                            Id = Guid.NewGuid(),
                            Name = googleUser.Name,
                            Email = googleUser.Email,
                            UserName = googleUser.Email,
                            EmailConfirmed = true,
                            EmailVerifiedAt = DateTime.UtcNow,
                            IsOAuthAccount = true,
                            OAuthProvider = "Google",
                            OAuthProviderId = googleUser.Sub,
                            ProfilePictureUrl = googleUser.Picture,
                            VerificationStatus = VerificationStatus.Unverified,
                            CreatedAt = DateTime.UtcNow
                        },
                        _ => new Patient
                        {
                            Id = Guid.NewGuid(),
                            FirstName = googleUser.GivenName,
                            LastName = googleUser.FamilyName,
                            Email = googleUser.Email,
                            UserName = googleUser.Email,
                            EmailConfirmed = true,
                            EmailVerifiedAt = DateTime.UtcNow,
                            IsOAuthAccount = true,
                            OAuthProvider = "Google",
                            OAuthProviderId = googleUser.Sub,
                            ProfilePictureUrl = googleUser.Picture,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    // Create user without password (OAuth account)
                    var result = await _userManager.CreateAsync(newUser);
                    if (!result.Succeeded)
                    {
                        return ApiResponse<AuthResponseDto>.Failure(
                            "Registration failed",
                            result.Errors.Select(e => e.Description),
                            400);
                    }

                    // Assign role
                    await EnsureRoleExistsAsync(userRole);
                    await _userManager.AddToRoleAsync(newUser, userRole.ToString());

                    // Send welcome email
                    await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FirstName ?? newUser.Email);

                    var authResponse = await GenerateAuthResponseAsync(newUser, ipAddress);

                    _logger.LogInformation("New user registered via Google: {Email}, Role: {Role}", newUser.Email, userRole);

                    return ApiResponse<AuthResponseDto>.Success(
                        authResponse,
                        "Registration successful! Welcome to Shuryan Healthcare.",
                        201);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google OAuth");
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred during Google login",
                    new[] { ex.Message },
                    500);
            }
        }

        #endregion
        #endregion

        #region Password Reset

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);

                // Don't reveal if user exists (security best practice)
                if (user == null || user.IsDeleted)
                {
                    return ApiResponse<bool>.Success(
                        true,
                        "If your email exists, you'll receive a password reset code");
                }


                // Check rate limiting
                var canResend = await _otpService.CanResendOtpAsync(dto.Email);
                if (!canResend)
                {
                    return ApiResponse<bool>.Failure(
                        "Too many requests",
                        new[] { "Please wait before requesting another code" },
                        429);
                }
                

                // Generate and send OTP
                var otpCode = await _otpService.GenerateAndStoreOtpAsync(
                    user.Id,
                    user.Email,
                    VerificationTypes.PasswordReset);

                await _emailService.SendPasswordResetOtpAsync(
                    user.Email,
                    user.FirstName,
                    otpCode);

                _logger.LogInformation("Password reset OTP sent to: {Email}", dto.Email);

                return ApiResponse<bool>.Success(
                    true,
                    "If your email exists, you'll receive a password reset code");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                return ApiResponse<bool>.Failure(
                    "An error occurred",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<bool>> VerifyResetOtpAndResetPasswordAsync(
            VerifyResetOtpRequest dto)
        {
            try
            {
                // Validate OTP
                var isValid = await _otpService.ValidateOtpAsync(
                    dto.Email,
                    dto.OtpCode,
                    VerificationTypes.PasswordReset);

                if (!isValid)
                {
                    return ApiResponse<bool>.Failure(
                        "Invalid or expired OTP code",
                        new[] { "Please check your code or request a new one" },
                        400);
                }

                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return ApiResponse<bool>.Failure("User not found", null, 404);
                }

                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.Failure(
                        "Password reset failed",
                        result.Errors.Select(e => e.Description),
                        400);
                }

                // Revoke all refresh tokens for security
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                    user.Id,
                    "Password reset");

                // Send confirmation email
                await _emailService.SendPasswordChangedNotificationAsync(
                    user.Email,
                    user.FirstName);

                _logger.LogInformation("Password reset successfully for: {Email}", dto.Email);

                return ApiResponse<bool>.Success(
                    true,
                    "Password reset successfully! You can now login with your new password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return ApiResponse<bool>.Failure(
                    "An error occurred during password reset",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(
            Guid userId,
            ChangePasswordRequest dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.IsDeleted)
                {
                    return ApiResponse<bool>.Failure("User not found", null, 404);
                }

                var result = await _userManager.ChangePasswordAsync(
                    user,
                    dto.CurrentPassword,
                    dto.NewPassword);

                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.Failure(
                        "Password change failed",
                        result.Errors.Select(e => e.Description),
                        400);
                }

                // Revoke all refresh tokens for security
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                    userId,
                    "Password changed");

                // Send notification email
                await _emailService.SendPasswordChangedNotificationAsync(
                    user.Email,
                    user.FirstName);

                _logger.LogInformation("Password changed for user: {UserId}", userId);

                return ApiResponse<bool>.Success(
                    true,
                    "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return ApiResponse<bool>.Failure(
                    "An error occurred while changing password",
                    new[] { ex.Message },
                    500);
            }
        }

        #endregion

        #region Token Management

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(
            RefreshTokenRequest dto,
            string? ipAddress = null)
        {
            try
            {
                // Extract user ID from access token (even if expired)
                var userId = _tokenService.GetUserIdFromExpiredToken(dto.AccessToken);

                if (userId == null)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Invalid token",
                        new[] { "Invalid access token" },
                        401);
                }

                // Find refresh token
                var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(dto.RefreshToken);

                if (refreshToken == null || refreshToken.UserId != userId)
                {
                    return ApiResponse<AuthResponseDto>.Failure(
                        "Invalid refresh token",
                        new[] { "Refresh token not found or does not match user" },
                        401);
                }

                // Check if active
                if (!refreshToken.IsActive)
                {
                    if (refreshToken.IsRevoked)
                    {
                        // Possible token reuse attack - revoke all user tokens
                        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                            userId.Value,
                            "Attempted reuse of revoked token");

                        _logger.LogWarning(
                            "Possible token reuse attack detected for user {UserId}",
                            userId);
                    }

                    return ApiResponse<AuthResponseDto>.Failure(
                        "Invalid refresh token",
                        new[] { "This refresh token is no longer valid" },
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

                _logger.LogInformation("Tokens refreshed for user: {UserId}", userId);

                return ApiResponse<AuthResponseDto>.Success(
                    authResponse,
                    "Token refreshed successfully",
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return ApiResponse<AuthResponseDto>.Failure(
                    "An error occurred while refreshing token",
                    new[] { ex.Message },
                    500);
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(
            string refreshToken,
            string? ipAddress = null)
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
                _logger.LogError(ex, "Error during logout");
                return ApiResponse<bool>.Failure(
                    "An error occurred during logout",
                    new[] { ex.Message },
                    500);
            }
        }

        #endregion

        #region User Info

        public async Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.IsDeleted)
                {
                    return ApiResponse<UserInfoDto>.Failure(
                        "User not found",
                        new[] { "User account not found" },
                        404);
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userInfo = await BuildUserInfoAsync(user, roles);

                return ApiResponse<UserInfoDto>.Success(
                    userInfo,
                    "User retrieved successfully",
                    200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user info");
                return ApiResponse<UserInfoDto>.Failure(
                    "An error occurred while retrieving user",
                    new[] { ex.Message },
                    500);
            }
        }

        #endregion

        #region Helper Methods

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(
            User user,
            string? ipAddress = null,
            bool rememberMe = false)
        {
            var roles = await _userManager.GetRolesAsync(user);

            // Generate access token
            var accessToken = _tokenService.GenerateAccessToken(
                user.Id,
                user.Email!,
                roles);

            // Generate refresh token
            var refreshTokenString = _tokenService.GenerateRefreshToken();

            // Calculate expiration
            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(
                _jwtSettings.AccessTokenExpirationMinutes);
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(
                rememberMe ? _jwtSettings.RefreshTokenExpirationDays * 2 : _jwtSettings.RefreshTokenExpirationDays);

            // Store refresh token
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
                Roles = roles,
                Role = roles.FirstOrDefault() ?? string.Empty, // Primary role for frontend
                ProfileImage = user is ProfileUser pUser ? (pUser.ProfileImageUrl ?? user.ProfilePictureUrl) : user.ProfilePictureUrl,
                AdditionalInfo = new Dictionary<string, object>
                {
                    { "EmailVerified", user.EmailConfirmed },
                    { "IsOAuthAccount", user.IsOAuthAccount }
                }
            };

            // Add type-specific info
            if (user is Doctor doctor)
            {
                userInfo.AdditionalInfo.Add("MedicalSpecialty", doctor.MedicalSpecialty.ToString());
                userInfo.AdditionalInfo.Add("VerificationStatus", doctor.VerificationStatus.ToString());
                userInfo.AdditionalInfo.Add("YearsOfExperience", doctor.YearsOfExperience);
            }
            else if (user is Laboratory lab)
            {
                userInfo.AdditionalInfo.Add("Name", lab.Name);
                userInfo.AdditionalInfo.Add("VerificationStatus", lab.VerificationStatus.ToString());
                userInfo.AdditionalInfo.Add("OffersHomeSampleCollection", lab.OffersHomeSampleCollection);
            }
            else if (user is Pharmacy pharmacy)
            {
                userInfo.AdditionalInfo.Add("Name", pharmacy.Name);
                userInfo.AdditionalInfo.Add("VerificationStatus", pharmacy.VerificationStatus.ToString());
                userInfo.AdditionalInfo.Add("OffersDelivery", pharmacy.OffersDelivery);
            }
            else if (user is Patient patient)
            {
                userInfo.AdditionalInfo.Add("BirthDate", patient.BirthDate?.ToString("yyyy-MM-dd") ?? "N/A");
                userInfo.AdditionalInfo.Add("Gender", patient.Gender?.ToString() ?? "N/A");
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

        #endregion

        #region Account Management

        public async Task<ApiResponse<bool>> DeleteAccountAsync(Guid userId, DeleteAccountRequest dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.IsDeleted)
                {
                    return ApiResponse<bool>.Failure("User not found", null, 404);
                }

                // Verify email matches
                if (!user.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<bool>.Failure(
                        "Email does not match",
                        new[] { "The email you provided does not match your account email" },
                        400);
                }

                // Password verification based on account type
                if (!user.IsOAuthAccount)
                {
                    // Non-OAuth accounts MUST provide password
                    if (string.IsNullOrEmpty(dto.Password))
                    {
                        return ApiResponse<bool>.Failure(
                            "Password required",
                            new[] { "Password is required for non-OAuth accounts" },
                            400);
                    }

                    var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
                    if (!passwordValid)
                    {
                        return ApiResponse<bool>.Failure(
                            "Invalid password",
                            new[] { "The password you provided is incorrect" },
                            401);
                    }
                }
                // OAuth accounts (Google, etc.) don't need password verification

                // Verify confirmation text
                if (!dto.ConfirmationText.Trim().Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<bool>.Failure(
                        "Invalid confirmation",
                        new[] { "You must type 'DELETE' to confirm account deletion" },
                        400);
                }

                _logger.LogWarning("ACCOUNT DELETION: Starting permanent deletion for user {UserId}, Email: {Email}", 
                    userId, user.Email);

                // HARD DELETE - Remove all user data permanently
                
                // 1. Delete all refresh tokens
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId, "Account deleted");
                
                // 2. Delete the user from Identity
                var deleteResult = await _userManager.DeleteAsync(user);
                
                if (!deleteResult.Succeeded)
                {
                    _logger.LogError("Failed to delete user {UserId}: {Errors}", 
                        userId, 
                        string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                    
                    return ApiResponse<bool>.Failure(
                        "Account deletion failed",
                        deleteResult.Errors.Select(e => e.Description),
                        500);
                }

                _logger.LogWarning("ACCOUNT DELETION: Successfully deleted user {UserId}, Email: {Email}", 
                    userId, user.Email);

                return ApiResponse<bool>.Success(
                    true,
                    "Account permanently deleted. You can now create a new account with the same email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during account deletion for user {UserId}", userId);
                return ApiResponse<bool>.Failure(
                    "An error occurred while deleting your account",
                    new[] { ex.Message },
                    500);
            }
        }

        /// <summary>
        /// [DEBUG ONLY] Delete account by email without authentication
        /// ⚠️ WARNING: For debugging/testing only! Remove before production!
        /// </summary>
        public async Task<ApiResponse<bool>> DebugDeleteAccountByEmailAsync(DeleteAccountRequest dto)
        {
            try
            {
                _logger.LogWarning("⚠️ DEBUG METHOD: Delete account by email: {Email}", dto.Email);

                // Find user by email
                var user = await _userManager.FindByEmailAsync(dto.Email);

                if (user == null || user.IsDeleted)
                {
                    return ApiResponse<bool>.Failure("User not found", null, 404);
                }

                // Call the regular delete method with user's ID
                return await DeleteAccountAsync(user.Id, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DEBUG: Error during account deletion by email for {Email}", dto.Email);
                return ApiResponse<bool>.Failure(
                    "An error occurred while deleting your account",
                    new[] { ex.Message },
                    500);
            }
        }

        #endregion
    }
}