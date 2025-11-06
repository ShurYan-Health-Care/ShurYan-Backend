using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Enums.Identity;

namespace Shuryan.Core.Entities.Identity
{
    public abstract class ProfileUser : User
	{
		public DateTime? BirthDate { get; set; }
		public Gender? Gender { get; set; }
		public string? ProfileImageUrl { get; set; }
	}
}
