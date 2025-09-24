using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Identity
{
	public abstract class ProfileUser : User
	{
		public DateTime? BirthDate { get; set; } // Optional for Credibility
		public Gender? Gender { get; set; } // Optional for Credibility
		public string? ProfileImageUrl { get; set; } // URL to the user's profile image -> Optional for Credibility
	}
}
