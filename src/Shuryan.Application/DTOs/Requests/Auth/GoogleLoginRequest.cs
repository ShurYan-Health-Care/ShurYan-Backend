using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Auth
{
    /// <summary>
    /// Request DTO for Google OAuth Login/Registration
    /// </summary>
    public class GoogleLoginRequest
    {
        /// <summary>
        /// Google ID Token from OAuth flow
        /// </summary>
        [Required(ErrorMessage = "Google ID token is required")]
        public string IdToken { get; set; } = string.Empty;

        /// <summary>
        /// User type for registration (patient, doctor, pharmacy, laboratory)
        /// Required only when registering a new user
        /// Possible values: "patient", "doctor", "pharmacy", "laboratory"
        /// </summary>
        public string? UserType { get; set; }
    }
}
