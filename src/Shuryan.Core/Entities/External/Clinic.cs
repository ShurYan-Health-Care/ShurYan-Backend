using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.External
{
    public class Clinic
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ClinicStatus Status { get; set; } = ClinicStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? FacilityVideoUrl { get; set; }

        [ForeignKey("DoctorClinic")]
        public Guid DoctorId { get; set; }

        [ForeignKey("Address")]
        public Guid AddressId { get; set; }

        // Navigation Properties
        public virtual Doctor DoctorClinic { get; set; } = null!;
        public virtual Address Address { get; set; } = null!;
        public virtual ICollection<ClinicPhoto> Photos { get; set; } = new HashSet<ClinicPhoto>();
        public virtual ICollection<ClinicPhoneNumber> PhoneNumbers { get; set; } = new HashSet<ClinicPhoneNumber>();
        public virtual ICollection<ClinicService> OfferedServices { get; set; } = new HashSet<ClinicService>();
    }
}
