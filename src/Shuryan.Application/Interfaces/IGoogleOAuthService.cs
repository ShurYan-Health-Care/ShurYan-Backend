using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Responses.Auth;

namespace Shuryan.Application.Interfaces
{
    public interface IGoogleOAuthService
    {
        Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken);
    }
}
