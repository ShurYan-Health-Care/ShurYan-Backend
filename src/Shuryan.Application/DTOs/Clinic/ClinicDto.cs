using Shuryan.Application.DTOs.Base;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Clinic
{
    public class ClinicDto : BaseAuditableDto
    {
        public string Name { get; set; } = string.Empty;
        public Status ClinicStatus { get; set; }
        public string? FacilityVideoUrl { get; set; }
        public Guid DoctorId { get; set; }
        public Guid AddressId { get; set; }

        public AddressDto? Address { get; set; }
        public IEnumerable<ClinicPhotoDto> Photos { get; set; } = new List<ClinicPhotoDto>();
        public IEnumerable<ClinicPhoneNumberDto> PhoneNumbers { get; set; } = new List<ClinicPhoneNumberDto>();
        public IEnumerable<ClinicServiceDto> OfferedServices { get; set; } = new List<ClinicServiceDto>();
    }

}
