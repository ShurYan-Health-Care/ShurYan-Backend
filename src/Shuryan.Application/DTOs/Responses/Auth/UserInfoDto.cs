using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Auth
{
    /// <summary>
    /// User information included in auth response
    /// </summary>
    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public Dictionary<string, object>? AdditionalInfo { get; set; }
    }
}
