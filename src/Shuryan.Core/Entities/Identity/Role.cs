using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shuryan.Core.Enums.Identity;

namespace Shuryan.Core.Entities.Identity
{
    public class Role : IdentityRole<Guid>
	{
		public UserRole UserRole { get; set; }
	}
}
