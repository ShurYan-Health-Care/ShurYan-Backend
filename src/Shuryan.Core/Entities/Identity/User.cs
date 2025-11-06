using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Identity
{
	public abstract class User : IdentityUser<Guid>
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;

		public DateTime CreatedAt { get; set; }
		public Guid? CreatedBy { get; set; } // User ID who created this record

		public DateTime? UpdatedAt { get; set; }
		public Guid? UpdatedBy { get; set; } // User ID who last updated this record

		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public Guid? DeletedBy { get; set; } // User ID who deleted this record

		[Phone, MaxLength(20)]
		public override string? PhoneNumber { get; set; }

		[Required, EmailAddress, MaxLength(200)]
		public override string Email { get; set; } = string.Empty;

        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }

        // ==================== OAuth Fields ====================

        public string? OAuthProvider { get; set; }
        public string? OAuthProviderId { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsOAuthAccount { get; set; } = false;
    }
}