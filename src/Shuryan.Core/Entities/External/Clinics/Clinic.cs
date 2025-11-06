using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Identity;

namespace Shuryan.Core.Entities.External.Clinic
{
    public class Clinic : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public Status ClinicStatus { get; set; } = Status.Active;
        public string? FacilityVideoUrl { get; set; }

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }

        [ForeignKey("Address")]
        public Guid AddressId { get; set; }

        // Navigation Properties
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Address Address { get; set; } = null!;
        public virtual ICollection<ClinicPhoto> Photos { get; set; } = new HashSet<ClinicPhoto>();
        public virtual ICollection<ClinicPhoneNumber> PhoneNumbers { get; set; } = new HashSet<ClinicPhoneNumber>();
        public virtual ICollection<ClinicService> OfferedServices { get; set; } = new HashSet<ClinicService>();
    }
}
