using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Identity
{
	public class Doctor : ProfileUser
	{
		public MedicalSpecialty MedicalSpecialty { get; set; }
		public int YearsOfExperience { get; set; }

		// Optional for Credibility
		public string? Biography { get; set; }

		// Verification
		public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;
		public DateTime? VerifiedAt { get; set; }

		[ForeignKey("Verifier")]
		public Guid? VerifierId { get; set; }

		// Navigation Properties
		public virtual DoctorVerifier? Verifier { get; set; }
		public virtual Clinic? Clinic { get; set; }
		public virtual ICollection<DoctorService> Services { get; set; } = new HashSet<DoctorService>();
		public virtual ICollection<DoctorOverride> Overrides { get; set; } = new HashSet<DoctorOverride>();
		public virtual ICollection<DoctorAvailability> Availabilities { get; set; } = new HashSet<DoctorAvailability>();
		public virtual ICollection<VerificationDocument> VerificationDocuments { get; set; } = new HashSet<VerificationDocument>();
	}
}