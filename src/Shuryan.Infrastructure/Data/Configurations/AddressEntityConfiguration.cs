using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shuryan.Core.Entities.Shared;
using Shuryan.Infrastructure.Data.Configurations.BaseConfigurations;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class AddressEntityConfiguration : SoftDeletableEntityConfiguration<Address>
	{
		public override void Configure(EntityTypeBuilder<Address> builder)
		{
			base.Configure(builder);

			// Table Mapping
			builder.ToTable("Addresses");

			// Properties
			builder.Property(a => a.Street).IsRequired().HasMaxLength(200);
			builder.Property(a => a.City).IsRequired().HasMaxLength(100);
			builder.Property(a => a.Governorate).IsRequired().HasConversion<int>();
			builder.Property(a => a.BuildingNumber).IsRequired(false).HasMaxLength(50);
			builder.Property(a => a.Latitude).IsRequired(false).HasPrecision(18, 12);
			builder.Property(a => a.Longitude).IsRequired(false).HasPrecision(18, 12);

			// Indexes
			builder.HasIndex(a => a.Governorate)
				.HasDatabaseName("IX_Address_Governorate");

			builder.HasIndex(a => new { a.Latitude, a.Longitude })
				.HasDatabaseName("IX_Address_Coordinates");
		}
	}
}
