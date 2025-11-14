using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shuryan.Shared.Configurations;

namespace Shuryan.Application.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public string GenerateAccessToken(
            Guid userId,
            string email,
            IEnumerable<string> roles,
            Dictionary<string, string>? additionalClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add additional claims if provided
            if (additionalClaims != null)
            {
                foreach (var claim in additionalClaims)
                {
                    claims.Add(new Claim(claim.Key, claim.Value));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return _tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = _jwtSettings.ValidateIssuer,
                    ValidateAudience = _jwtSettings.ValidateAudience,
                    ValidateLifetime = _jwtSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes)
                };

                var principal = _tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Verify it's a JWT token with the correct algorithm
                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public Guid? GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null) return null;

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                                  ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public string? GetEmailFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null) return null;

                var emailClaim = principal.FindFirst(ClaimTypes.Email)
                                 ?? principal.FindFirst(JwtRegisteredClaimNames.Email);

                return emailClaim?.Value;
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<Claim> GetClaimsFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                return principal?.Claims ?? Enumerable.Empty<Claim>();
            }
            catch
            {
                return Enumerable.Empty<Claim>();
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true; // If we can't read it, consider it expired
            }
        }

        public Guid? GetUserIdFromExpiredToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = _jwtSettings.ValidateIssuer,
                    ValidateAudience = _jwtSettings.ValidateAudience,
                    ValidateLifetime = false, 
                    ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = _tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Verify it's a JWT token with the correct algorithm
                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                                      ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);

                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                    {
                        return userId;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}