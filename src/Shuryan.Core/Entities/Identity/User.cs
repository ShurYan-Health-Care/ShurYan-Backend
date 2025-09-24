using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Identity
{
	public abstract class User : IdentityUser<Guid>
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public bool IsActive { get; set; } = true; // Soft delete -> if false, the user is considered deleted
		public UserRole UserRole { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Nessessary for Reports
		public DateTime? UpdatedAt { get; set; } // Records the last time the user updated any of their account details.
	}
}