using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Auth
{
    public class GoogleLoginRequest
    {
        [Required(ErrorMessage = "Google ID token is required")]
        public string IdToken { get; set; } = string.Empty;

        /// <summary>
        /// Optional user role for registration (Patient, Doctor, etc.)
        /// Defaults to Patient if not specified
        /// </summary>
        public string? UserRole { get; set; }
    }
}
