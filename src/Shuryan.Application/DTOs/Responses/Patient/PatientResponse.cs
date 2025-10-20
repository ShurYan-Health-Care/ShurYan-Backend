using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Patient
{
    public class PatientResponse : BaseAuditableDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public string? ProfileImageUrl { get; set; }
        public AddressResponse? Address { get; set; }
        public IEnumerable<MedicalHistoryItemResponse> MedicalHistory { get; set; } = new List<MedicalHistoryItemResponse>();
    }
}

