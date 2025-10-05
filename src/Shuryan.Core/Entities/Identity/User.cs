using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
		public DateTime CreatedAt { get; set; } // Nessessary for Reports
		public DateTime? UpdatedAt { get; set; } // Records the last time the user updated any of their account details.

		[Phone, MaxLength(20)]
		public override string? PhoneNumber { get; set; }

		[Required, EmailAddress, MaxLength(200)]
		public override string Email { get; set; } = string.Empty;
	}
}