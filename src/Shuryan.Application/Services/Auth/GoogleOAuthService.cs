using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Server;
using Shuryan.Application.DTOs.Responses.Auth;
using Shuryan.Application.Interfaces;
using Shuryan.Shared.Configurations;

namespace Shuryan.Application.Services.Auth
{
    public class GoogleOAuthService : IGoogleOAuthService
    {
        private readonly OAuthSettings _oauthSettings;
        private readonly ILogger<GoogleOAuthService> _logger;

        public GoogleOAuthService(
            IOptions<OAuthSettings> oauthSettings,
            ILogger<GoogleOAuthService> logger)
        {
            _oauthSettings = oauthSettings.Value;
            _logger = logger;
        }

        public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                if (!_oauthSettings.Google.Enabled)
                {
                    _logger.LogWarning("Google OAuth is disabled in configuration");
                    return null;
                }

                // Validate Google ID token
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _oauthSettings.Google.ClientId }
                    });

                if (payload == null)
                {
                    _logger.LogWarning("Invalid Google ID token");
                    return null;
                }

                // Map to our DTO
                var userInfo = new GoogleUserInfo
                {
                    Sub = payload.Subject,
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    Name = payload.Name ?? "",
                    GivenName = payload.GivenName ?? "",
                    FamilyName = payload.FamilyName ?? "",
                    Picture = payload.Picture ?? "",
                    Locale = payload.Locale ?? "en"
                };

                _logger.LogInformation("Google token validated successfully for email {Email}", userInfo.Email);
                return userInfo;
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogError(ex, "Invalid JWT token from Google");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token");
                return null;
            }
        }
    }
}