using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.External;

namespace Shuryan.Infrastructure.Data.Configurations
{
    public class AddressEntityConfiguration : IEntityTypeConfiguration<Address>
	{
		public void Configure(EntityTypeBuilder<Address> builder)
		{
			builder.HasKey(a => a.Id);

			builder.Property(a => a.Street)
				   .IsRequired()
				   .HasMaxLength(200);

			builder.Property(a => a.City)
				   .IsRequired()
				   .HasMaxLength(100);

			builder.Property(a => a.BuildingNumber)
				   .HasMaxLength(50);

			builder.Property(a => a.Governorate)
				   .HasConversion<int>()
				   .IsRequired();

			builder.Property(a => a.Latitude)
				   .HasPrecision(18, 12);

			builder.Property(a => a.Longitude)
				   .HasPrecision(18, 12);

			// Relationships
			builder.HasOne(a => a.Clinic)
				   .WithOne(c => c.Address)
				   .HasForeignKey<Clinic>(c => c.AddressId)
				   .OnDelete(DeleteBehavior.Restrict);

			// Indexes for performance
			builder.HasIndex(a => a.Governorate)
				   .HasDatabaseName("IX_Address_Governorate");

			builder.HasIndex(a => new { a.Latitude, a.Longitude })
				   .HasDatabaseName("IX_Address_Coordinates");
		}
	}
}
