using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.External;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Shared
{
    public class Address : SoftDeletableEntity
	{
		public string Street { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public Governorate Governorate { get; set; }
		public string? BuildingNumber { get; set; }

		// Geographic Coordinates
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
	}
}