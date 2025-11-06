using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Shared
{
    public class Address : SoftDeletableEntity
	{
		public string Street { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public Governorate Governorate { get; set; }
		public string? BuildingNumber { get; set; }
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
	}
}