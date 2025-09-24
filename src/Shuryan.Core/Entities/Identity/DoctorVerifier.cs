using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Identity
{
	public class DoctorVerifier : User
	{
		public Guid CreatedByAdminId { get; set; }

		public DoctorVerifier()
		{
			UserRole = UserRole.Verifier;
		}

		public virtual ICollection<Doctor> VerifiedDoctors { get; set; } = new HashSet<Doctor>();
	}
}
