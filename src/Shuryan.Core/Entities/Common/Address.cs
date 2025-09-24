using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Common
{
	public class Address
	{
		public Guid Id { get; set; }
		public string Street { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public Governorate Governorate { get; set; }
		public string? BuildingNumber { get; set; }

		// Geographic Coordinates
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }

		// Navigation Properties
		// Address with Patient -> Optional 1:1
		// Address with Clinic -> Mandatory 1:1
		public virtual Patient? Patient { get; set; } = null!;
		public virtual Clinic? Clinic { get; set; }
	}
}