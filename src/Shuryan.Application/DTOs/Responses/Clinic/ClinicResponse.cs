using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Clinic
{
    public class ClinicResponse : BaseAuditableDto
    {
        public string Name { get; set; } = string.Empty;
        public Status ClinicStatus { get; set; }
        public string? FacilityVideoUrl { get; set; }
        public Guid DoctorId { get; set; }
        public Guid AddressId { get; set; }

        public AddressResponse? Address { get; set; }
        public IEnumerable<ClinicPhotoResponse> Photos { get; set; } = new List<ClinicPhotoResponse>();
        public IEnumerable<ClinicPhoneNumberResponse> PhoneNumbers { get; set; } = new List<ClinicPhoneNumberResponse>();
        public IEnumerable<ClinicServiceResponse> OfferedServices { get; set; } = new List<ClinicServiceResponse>();
    }
}

