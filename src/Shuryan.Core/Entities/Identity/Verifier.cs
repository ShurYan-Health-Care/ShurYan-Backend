using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Identity
{
	public class Verifier : User
	{
		public Guid CreatedByAdminId { get; set; } // حاليا مفيش يوزر ادمن ف فمفيش ربط حاليا

		public virtual ICollection<Doctor> VerifiedDoctors { get; set; } = new HashSet<Doctor>();
		public virtual ICollection<Laboratory> VerifiedLabors { get; set;} = new HashSet<Laboratory>();
	}
}
