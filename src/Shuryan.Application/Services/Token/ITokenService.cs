using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.Services.Token
{
    public interface ITokenService
    {
        string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles, Dictionary<string, string>? additionalClaims = null);

        string GenerateRefreshToken();

        ClaimsPrincipal? ValidateToken(string token);

        Guid? GetUserIdFromToken(string token);

        string? GetEmailFromToken(string token);

        IEnumerable<Claim> GetClaimsFromToken(string token);

        bool IsTokenExpired(string token);

        Guid? GetUserIdFromExpiredToken(string token);
    }
}