using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Auth
{
    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public string Role { get; set; } = string.Empty; // Primary role (Patient, Doctor, Pharmacy, Laboratory)
        public Dictionary<string, object>? AdditionalInfo { get; set; }
    }
}
