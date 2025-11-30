using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Verifier
{
    public class VerifierResponse : BaseAuditableDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        //public Guid CreatedByAdminId { get; set; }
    }
}
